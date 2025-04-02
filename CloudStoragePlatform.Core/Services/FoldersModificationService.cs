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
        private readonly SSE sse;
        public FoldersModificationService(IFoldersRepository foldersRepository, IFilesRepository filesRepository) 
        {
            _foldersRepository = foldersRepository;
            _filesRepository = filesRepository;
            sse = new SSE();
            // inject user identifying stuff in constructor and in repository's constructor
        }
        public async Task<FolderResponse> AddFolder(FolderAddRequest folderAddRequest, bool skipSSE = false)
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

                Folder? parent = await _foldersRepository.GetFolderByFolderPath(parentFolderPath);

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
            if (!skipSSE)
            {
                await sse.SendEventAsync("add", response);
            }
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
            await sse.SendEventAsync("favorite_updated", new {id=fid, val=folder.IsFavorite});
            return updatedFolder!.ToFolderResponse();
        }

        public async Task<FolderResponse> AddOrRemoveTrash(Guid fid, bool skipSSE = false)
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
            
            if (!skipSSE) 
            {
                await sse.SendEventAsync("trash_updated", new { id = fid, val = folder.IsTrash });
            }
            
            return updatedFolder!.ToFolderResponse();
        }

        public async Task<bool> DeleteFolder(Guid fid, bool skipSSE = false)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(fid);
            if (folder == null) 
            {
                throw new ArgumentException();
            }
            Directory.Delete(folder.FolderPath, true);
            if (!skipSSE)
            {
                await sse.SendEventAsync("deleted", new { id = fid });
            }
            return await _foldersRepository.DeleteFolder(folder);
        }

        // just confirming that including folder to moved's name in newFolderPath will NOT proof code from moving folder to its subfolder
        public async Task<FolderResponse> MoveFolder(Guid folderId, string newFolderPath, bool skipSSE = false)
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
            if (!skipSSE)
            {
                await sse.SendEventAsync("moved", new
                {
                    movedTo = newFolderPathOfFolder,
                    id = folderId,
                    res = response
                });
            }
            return response;
        }

        public async Task<FolderResponse> RenameFolder(FolderRenameRequest folderRenameRequest)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(folderRenameRequest.FolderId);
            if (folder == null) 
            {
                throw new ArgumentException();
            }
            string newp = Utilities.ReplaceLastOccurance(folder.FolderPath, folder.FolderName, folderRenameRequest.FolderNewName);
            if (Directory.Exists(newp)) 
            {
                throw new DuplicateFolderException();
            }
            string oldp = folder!.FolderPath;
            folder.FolderName = folderRenameRequest.FolderNewName;
            folder.FolderPath = newp;
            await Utilities.UpdateChildPaths(_foldersRepository, _filesRepository, folder, oldp, newp);
            await Utilities.UpdateMetadataRename(folder, _foldersRepository);
            Folder? updatedFolder = await _foldersRepository.UpdateFolder(folder, true, false, false, false, false, false);
            FileSystem.RenameDirectory(oldp, folderRenameRequest.FolderNewName);
            await sse.SendEventAsync("rename", new { id = folderRenameRequest.FolderId, val = folder.FolderName });
            return updatedFolder!.ToFolderResponse();
        }
    }
}
