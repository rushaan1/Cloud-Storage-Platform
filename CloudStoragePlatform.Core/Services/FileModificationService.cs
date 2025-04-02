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
        private readonly SSE sse;
        public FileModificationService(IFoldersRepository foldersRepository, IFilesRepository filesRepository)
        {
            _foldersRepository = foldersRepository;
            _filesRepository = filesRepository;
            sse = new SSE();
            // inject user identifying stuff in constructor and in repository's constructor
        }

        public async Task<FileResponse> UploadFile(FileAddRequest fileAddRequest, Stream stream, bool skipSSE = false)
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
                    ReplaceCount = 0,
                    RenameCount = 0,
                    MoveCount = 0,
                    OpenCount = 0,
                    ShareCount = 0,
                    Size = 0
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

                metadata.File = file;
                sharing.File = file;
                await _filesRepository.AddFile(file);
                if (parent != null)
                {
                    parent.Files.Add(file);
                    await _foldersRepository.UpdateFolder(parent, false, false, false, false, false, true);
                }
                using (FileStream fs = new FileStream(file.FilePath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fs);
                }
            }
            else
            {
                throw new ArgumentException();
            }
            var response = file.ToFileResponse();
            if (skipSSE) 
            {
                await sse.SendEventAsync("add", response);
            }
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
            await sse.SendEventAsync("favorite_updated", new { id = fileId, val = file.IsFavorite });
            return updatedFile!.ToFileResponse();
        }

        public async Task<FileResponse> AddOrRemoveTrash(Guid fileId, bool skipSSE = false)
        {
            var file = await _filesRepository.GetFileByFileId(fileId);
            if (file == null)
            {
                throw new ArgumentException();
            }

            file.IsTrash = !file.IsTrash;

            var updatedFile = await _filesRepository.UpdateFile(file, true, false, false, false);
            if (!skipSSE)
            {
                await sse.SendEventAsync("trash_updated", new { id = fileId, val = file.IsTrash });
            }
            return updatedFile!.ToFileResponse();
        }

        public async Task<bool> DeleteFile(Guid fileId, bool skipSSE = false)
        {
            var file = await _filesRepository.GetFileByFileId(fileId);
            if (file == null)
            {
                throw new ArgumentException();
            }
            System.IO.File.Delete(file.FilePath);
            if (!skipSSE)
            {
                await sse.SendEventAsync("deleted", new { id = fileId });
            }
            return await _filesRepository.DeleteFile(file);
        }

        public async Task<FileResponse> MoveFile(Guid fileId, string newParentPath, bool skipSSE = false)
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

            Folder? newParent = await _foldersRepository.GetFolderByFolderPath(newParentPath);


            file.FilePath = newFilePathOfFile;
            file.ParentFolder = newParent!;

            File? finalMainFile = await _filesRepository.UpdateFile(file, true, true, false, false);
            System.IO.File.Move(previousFilePath, newFilePathOfFile);
            await Utilities.UpdateMetadataMove(file, previousFilePath, _filesRepository);
            var response = finalMainFile!.ToFileResponse();
            if (!skipSSE)
            {
                await sse.SendEventAsync("moved", new
                {
                    movedTo = newFilePathOfFile,
                    id = fileId,
                    res = response
                });
            }
            return response;
        }

        public async Task<FileResponse> RenameFile(FileRenameRequest fileRenameRequest)
        {
            var file = await _filesRepository.GetFileByFileId(fileRenameRequest.FileId);
            if (file == null)
            {
                throw new ArgumentException();
            }

            string newFilePath = Path.Combine(Path.GetDirectoryName(file.FilePath)!, fileRenameRequest.FileNewName);
            if (System.IO.File.Exists(newFilePath))
            {
                throw new DuplicateFileException();
            }

            string oldFilePath = file.FilePath;
            file.FileName = fileRenameRequest.FileNewName;
            file.FilePath = newFilePath;

            await Utilities.UpdateMetadataRename(file, _filesRepository);
            var updatedFile = await _filesRepository.UpdateFile(file, true, false, false, false);
            System.IO.File.Move(oldFilePath, newFilePath);
            await sse.SendEventAsync("rename", new { id = fileRenameRequest.FileId, val = file.FileName});
            return updatedFile!.ToFileResponse();
        }
    }
}
