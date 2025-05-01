using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using CloudStoragePlatform.Core.Exceptions;
using CloudStoragePlatform.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using File = CloudStoragePlatform.Core.Domain.Entities.File;

namespace CloudStoragePlatform.Core.Services
{
    public class FileModificationService : IFilesModificationService
    {
        private readonly IFoldersRepository _foldersRepository;
        private readonly IFilesRepository _filesRepository;
        private readonly SSE _sse;

        public FileModificationService(IFoldersRepository foldersRepository, IFilesRepository filesRepository, SSE sse)
        {
            _foldersRepository = foldersRepository;
            _filesRepository = filesRepository;
            _sse = sse;
            // inject user identifying stuff in constructor and in repository's constructor
        }

        private async Task UpdateFolderSizesOnAdd(Folder? folder, float sizeInMB)
        {
            if (folder == null)
                return;

            // Update with 2 decimal precision
            folder.Size = (float)Math.Round(folder.Size + sizeInMB, 2);
            await _foldersRepository.UpdateFolder(folder, true, false, false, false, false, false);
            
            // Send SSE notification about folder size update
            await _sse.SendEventAsync("size_updated", new { id = folder.FolderId, size = folder.Size });
            
            // Recursively update parent folders
            if (folder.ParentFolder != null)
            {
                await UpdateFolderSizesOnAdd(folder.ParentFolder, sizeInMB);
            }
        }

        private async Task UpdateFolderSizesOnDelete(Folder? folder, float sizeInMB)
        {
            if (folder == null)
                return;

            // Update with 2 decimal precision, ensuring it doesn't go below 0
            folder.Size = (float)Math.Round(Math.Max(0, folder.Size - sizeInMB), 2);
            await _foldersRepository.UpdateFolder(folder, true, false, false, false, false, false);
            
            // Send SSE notification about folder size update
            await _sse.SendEventAsync("size_updated", new { id = folder.FolderId, size = folder.Size });
            
            // Recursively update parent folders
            if (folder.ParentFolder != null)
            {
                await UpdateFolderSizesOnDelete(folder.ParentFolder, sizeInMB);
            }
        }

        private float ConvertBytesToMegabytes(long bytes)
        {
            // Convert bytes to megabytes with 2 decimal precision
            return (float)Math.Round((float)bytes / (1024 * 1024), 2);
        }

        public async Task<FileResponse> UploadFile(FileAddRequest fileAddRequest, Stream stream)
        {
            string parentFolderPath = Utilities.ReplaceLastOccurance(fileAddRequest.FilePath, @"\" + fileAddRequest.FileName, "");
            File? file = null;
            if (Directory.Exists(parentFolderPath))
            {
                if (System.IO.File.Exists(fileAddRequest.FilePath))
                {
                    throw new DuplicateFileException();
                }
                Metadata metadata = new Metadata()
                {
                    MetadataId = Guid.NewGuid(),
                    RenameCount = 0,
                    MoveCount = 0,
                    OpenCount = 0,
                    ShareCount = 0
                };
                Sharing sharing = new Sharing()
                {
                    SharingId = Guid.NewGuid(),
                };

                string extension = fileAddRequest.FileName.Split('.').Last().ToLower();
                FileType fileType = extension switch
                {
                    "jpg" or "jpeg" or "png" or "webp" => FileType.Image,
                    "mp3" or "wav" => FileType.Audio,
                    "gif" => FileType.GIF,
                    "mp4" or "avi" => FileType.Video,
                    "pdf" or "doc" or "docx" or "txt" => FileType.Document,
                    _ => FileType.Document
                };

                Folder? parent = await _foldersRepository.GetFolderByFolderPath(parentFolderPath);
                if (fileAddRequest.FileName.Contains("\\"))
                {
                    // This is a very rare exceptional edge case as Linux allows file names to have \
                    string[] filesInParent = (string[])parent!.Files.Select((f) => { return f.FileName; });
                    string newName = Utilities.FindUniqueName(filesInParent, fileAddRequest.FileName.Replace("\\", "-"), true);
                    fileAddRequest.FilePath = Utilities.ReplaceLastOccurance(fileAddRequest.FilePath, fileAddRequest.FileName, newName);
                    fileAddRequest.FileName = newName;
                }

                
                file = new File()
                {
                    FileId = Guid.NewGuid(),
                    FileName = fileAddRequest.FileName,
                    FilePath = fileAddRequest.FilePath,
                    ParentFolder = parent,
                    Metadata = metadata,
                    Sharing = sharing,
                    CreationDate = DateTime.Now,
                    FileType = fileType
                };

                using (FileStream fs = new FileStream(file.FilePath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fs);
                }
                float fileSizeInMB = (float)Math.Round(GetFileSizeInMB(file.FilePath), 2);
                file.Size = fileSizeInMB;

                metadata.File = file;
                sharing.File = file;
                await _filesRepository.AddFile(file);
                
                if (parent != null)
                {
                    parent.Files.Add(file);
                    await _foldersRepository.UpdateFolder(parent, false, false, false, false, false, true);
                    await UpdateFolderSizesOnAdd(parent, fileSizeInMB);
                }
            }
            else
            {
                throw new ArgumentException();
            }
            ThumbnailService thumbnailService = new ThumbnailService();
            if (file.FileType == FileType.Image || file.FileType == FileType.GIF)
            {
                await thumbnailService.GenerateImageThumbnail(file.FileId, file.FilePath, file.FileType == FileType.GIF);
            }
            else if (file.FileType == FileType.Video)
            {
                await thumbnailService.GenerateVideoThumbnail(file.FileId, file.FilePath);
            }
            var response = file.ToFileResponse();
            return response;
        }

        public async Task<FileResponse> AddOrRemoveFavorite(Guid fileId)
        {
            var file = await _filesRepository.GetFileByFileId(fileId);
            if (file == null)
            {
                throw new ArgumentException();
            }

            file.IsFavorite = !file.IsFavorite;

            var updatedFile = await _filesRepository.UpdateFile(file, true, false, false, false);
            return updatedFile!.ToFileResponse();
        }

        public async Task<FileResponse> AddOrRemoveTrash(Guid fileId)
        {
            var file = await _filesRepository.GetFileByFileId(fileId);
            if (file == null)
            {
                throw new ArgumentException();
            }

            bool wasInTrash = file.IsTrash;
            file.IsTrash = !file.IsTrash;

            var updatedFile = await _filesRepository.UpdateFile(file, true, false, false, false);
            
            // Get the parent folder and file size
            Folder? parentFolder = file.ParentFolder;
            float fileSizeInMB = file.Size;
            
            // If file is being added to trash, subtract size from ancestors
            if (!wasInTrash && file.IsTrash && parentFolder != null)
            {
                await UpdateFolderSizesOnDelete(parentFolder, fileSizeInMB);
            }
            // If file is being removed from trash, add size back to ancestors
            else if (wasInTrash && !file.IsTrash && parentFolder != null)
            {
                await UpdateFolderSizesOnAdd(parentFolder, fileSizeInMB);
            }
            
            return updatedFile!.ToFileResponse();
        }

        public async Task<bool> DeleteFile(Guid fileId)
        {
            var file = await _filesRepository.GetFileByFileId(fileId);
            if (file == null)
            {
                throw new ArgumentException();
            }
            
            // Get the parent folder and file size before deleting
            Folder? parentFolder = file.ParentFolder;
            float fileSizeInMB = file.Size;
            
            // Delete the file from the file system
            System.IO.File.Delete(file.FilePath);
            
            // Delete the file from database
            bool result = await _filesRepository.DeleteFile(file);
            
            // Update sizes of parent folder and its ancestors
            if (result && parentFolder != null)
            {
                await UpdateFolderSizesOnDelete(parentFolder, fileSizeInMB);
            }
            
            return result;
        }

        public async Task<FileResponse> MoveFile(Guid fileId, string newParentPath)
        {
            var file = await _filesRepository.GetFileByFileId(fileId);
            if (file == null)
            {
                throw new ArgumentException();
            }
            if (Directory.Exists(newParentPath) == false)
            {
                throw new DirectoryNotFoundException();
            }

            string previousFilePath = file.FilePath;
            string newFilePathOfFile = Path.Combine(newParentPath, file.FileName);

            if (System.IO.File.Exists(newFilePathOfFile))
            {
                throw new DuplicateFileException();
            }

            // Store the old parent folder and file size
            Folder? oldParent = file.ParentFolder;
            float fileSizeInMB = file.Size;

            // Get new parent folder
            Folder? newParent = await _foldersRepository.GetFolderByFolderPath(newParentPath);

            file.FilePath = newFilePathOfFile;
            file.ParentFolder = newParent!;

            File? finalMainFile = await _filesRepository.UpdateFile(file, true, true, false, false);
            System.IO.File.Move(previousFilePath, newFilePathOfFile);
            await Utilities.UpdateMetadataMove(file, previousFilePath, _filesRepository);
            
            // Update folder sizes if the parent folder changes
            if (oldParent != null && newParent != null && oldParent.FolderId != newParent.FolderId)
            {
                // Decrease size from old parent
                await UpdateFolderSizesOnDelete(oldParent, fileSizeInMB);
                
                // Increase size for new parent
                await UpdateFolderSizesOnAdd(newParent, fileSizeInMB);
            }
            
            var response = finalMainFile!.ToFileResponse();
            return response;
        }

        public async Task<FileResponse> RenameFile(RenameRequest fileRenameRequest)
        {
            var file = await _filesRepository.GetFileByFileId(fileRenameRequest.id);
            if (file == null)
            {
                throw new ArgumentException();
            }

            string newFilePath = Path.Combine(Path.GetDirectoryName(file.FilePath)!, fileRenameRequest.newName);
            if (System.IO.File.Exists(newFilePath))
            {
                throw new DuplicateFileException();
            }

            string oldFilePath = file.FilePath;
            file.FileName = fileRenameRequest.newName;
            file.FilePath = newFilePath;

            await Utilities.UpdateMetadataRename(file, _filesRepository);
            var updatedFile = await _filesRepository.UpdateFile(file, true, false, false, false);
            System.IO.File.Move(oldFilePath, newFilePath);
            return updatedFile!.ToFileResponse();
        }

        private float GetFileSizeInMB(string filePath)
        {
            var fileInfo = new System.IO.FileInfo(filePath);
            return ConvertBytesToMegabytes(fileInfo.Length);
        }
    }
}
