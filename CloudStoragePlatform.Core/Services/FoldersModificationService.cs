using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Exceptions;
using CloudStoragePlatform.Core.ServiceContracts;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Services
{
    public class FoldersModificationService : IFoldersModificationService
    {
        private readonly IFoldersRepository _foldersRepository;
        private readonly IFilesRepository _filesRepository;
        public FoldersModificationService(IFoldersRepository foldersRepository, IFilesRepository filesRepository) 
        {
            _foldersRepository = foldersRepository;
            _filesRepository = filesRepository;
            // inject user identifying stuff in constructor and in repository's constructor
        }
        public async Task<FolderResponse> AddFolder(FolderAddRequest folderAddRequest)
        {
            
            string parentFolderPath = Utilities.ReplaceLastOccurance(folderAddRequest.FolderPath, @"\"+folderAddRequest.FolderName, "");
            Folder? folder = null;
            if (Directory.Exists(parentFolderPath))
            {
                if (Directory.Exists(folderAddRequest.FolderPath))
                {
                    throw new DuplicateFolderException();
                }
                Metadata metadata = new Metadata() 
                {
                    MetadataId = Guid.NewGuid(),
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

                Folder? parent = await _foldersRepository.GetFolderByFolderPath(parentFolderPath);
                if (folderAddRequest.FolderName.Contains("\\"))
                {
                    // This is a very rare exceptional edge case as Linux allows file names to have \
                    string[] foldersInParent = (string[])parent!.SubFolders.Select((f) => { return f.FolderName; });
                    string newName = Utilities.FindUniqueName(foldersInParent, folderAddRequest.FolderName.Replace("\\", "-"), true);
                    folderAddRequest.FolderPath = Utilities.ReplaceLastOccurance(folderAddRequest.FolderPath, folderAddRequest.FolderName, newName);
                    folderAddRequest.FolderName = newName;
                }
                folder = new Folder() { FolderId = Guid.NewGuid(), FolderName = folderAddRequest.FolderName, FolderPath = folderAddRequest.FolderPath, ParentFolder = parent, Metadata = metadata, Sharing = sharing, CreationDate = DateTime.Now };
                metadata.Folder = folder;
                sharing.Folder = folder;
                await _foldersRepository.AddFolder(folder);
                if (parent != null)
                {
                    parent.SubFolders.Add(folder);
                    await _foldersRepository.UpdateFolder(parent, false, false, false, false, true, false);
                }
                Directory.CreateDirectory(folderAddRequest.FolderPath);
            }
            else 
            {
                throw new ArgumentException();
            }
            FolderResponse response = folder.ToFolderResponse();
            return response;
        }

        public async Task<FolderResponse> AddOrRemoveFavorite(Guid fid)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(fid);
            if (folder == null) 
            {
                throw new ArgumentException();
            }
            if (folder.IsFavorite)
            {
                folder.IsFavorite = false;
            }
            else if (!folder.IsFavorite)
            {
                folder.IsFavorite = true;
            }
            Folder? updatedFolder = await _foldersRepository.UpdateFolder(folder, true, false, false, false, false, false);
            return updatedFolder!.ToFolderResponse();
        }

        public async Task<FolderResponse> AddOrRemoveTrash(Guid fid)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(fid);
            if (folder == null)
            {
                throw new ArgumentException();
            }
            if (folder.IsTrash)
            {
                folder.IsTrash = false;
            }
            else
            {
                folder.IsTrash = true;
            }
            Folder? updatedFolder = await _foldersRepository.UpdateFolder(folder, true, false, false, false, false, false);
            
            
            return updatedFolder!.ToFolderResponse();
        }

        public async Task<bool> DeleteFolder(Guid fid)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(fid);
            if (folder == null) 
            {
                throw new ArgumentException();
            }
            Directory.Delete(folder.FolderPath, true);
            return await _foldersRepository.DeleteFolder(folder);
        }

        // just confirming that including folder to moved's name in newFolderPath will NOT proof code from moving folder to its subfolder
        public async Task<FolderResponse> MoveFolder(Guid folderId, string newFolderPath)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(folderId);
            if (folder == null)
            {
                throw new ArgumentException();
            }
            if (Directory.Exists(newFolderPath) == false) 
            {
                throw new DirectoryNotFoundException();
            }


            string previousFolderPath = folder.FolderPath;
            string newFolderPathOfFolder = Path.Combine(newFolderPath, folder.FolderName);

            if (Directory.Exists(newFolderPathOfFolder))
            {
                throw new DuplicateFolderException();
            }
            
            Folder? newParent = await _foldersRepository.GetFolderByFolderPath(newFolderPath);

            if (newParent?.FolderId == folderId) 
            {
                throw new ArgumentException();
            }
            
            Folder? parent = newParent;
            while (parent != null)
            {
                if (parent.FolderId == folder.FolderId)
                {
                    throw new ArgumentException();
                }
                parent = parent.ParentFolder;
            }


            folder.FolderPath = newFolderPathOfFolder;
            folder.ParentFolder = newParent!;

            Folder? finalMainFolder = await _foldersRepository.UpdateFolder(folder, true, true, false, false, false, false);
            await Utilities.UpdateChildPaths(_foldersRepository,_filesRepository,folder, previousFolderPath, newFolderPathOfFolder);
            Directory.Move(previousFolderPath, newFolderPathOfFolder);
            await Utilities.UpdateMetadataMove(folder, previousFolderPath, _foldersRepository);
            var response = finalMainFolder!.ToFolderResponse();
            return response;
        }

        public async Task<FolderResponse> RenameFolder(RenameRequest folderRenameRequest)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(folderRenameRequest.id);
            if (folder == null) 
            {
                throw new ArgumentException();
            }
            string newp = Utilities.ReplaceLastOccurance(folder.FolderPath, folder.FolderName, folderRenameRequest.newName);
            if (Directory.Exists(newp)) 
            {
                throw new DuplicateFolderException();
            }
            string oldp = folder!.FolderPath;
            folder.FolderName = folderRenameRequest.newName;
            folder.FolderPath = newp;
            await Utilities.UpdateChildPaths(_foldersRepository, _filesRepository, folder, oldp, newp);
            await Utilities.UpdateMetadataRename(folder, _foldersRepository);
            Folder? updatedFolder = await _foldersRepository.UpdateFolder(folder, true, false, false, false, false, false);
            FileSystem.RenameDirectory(oldp, folderRenameRequest.newName);
            return updatedFolder!.ToFolderResponse();
        }
    }
}
