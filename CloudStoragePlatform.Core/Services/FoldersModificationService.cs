using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Exceptions;
using CloudStoragePlatform.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
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
        }
        public async Task<FolderResponse> AddFolder(FolderAddRequest folderAddRequest)
        {
            Folder? folder = null;
            if (Directory.Exists(folderAddRequest.FolderPath.Replace(folderAddRequest.FolderName, "")))
            {
                if (Directory.Exists(folderAddRequest.FolderPath))
                {
                    throw new DuplicateFolderException();
                }
                folder = new Folder() { FolderId = Guid.NewGuid(), FolderName = folderAddRequest.FolderName, FolderPath = folderAddRequest.FolderPath };
                await _foldersRepository.AddFolder(folder);
                Directory.CreateDirectory(folderAddRequest.FolderPath);
            }
            else 
            {
                throw new ArgumentException();
            }
            return folder.ToFolderResponse();
        }

        public async Task<FolderResponse> AddOrRemoveFavorite(Guid folderId)
        {
            throw new NotImplementedException();
        }

        public async Task<FolderResponse> AddOrRemoveTrash(Guid folderId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteFolder(Guid folderId)
        {
            throw new NotImplementedException();
        }

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
            
            Folder? newParent = await _foldersRepository.GetFolderByFolderPath(newFolderPath);
            folder.FolderPath = newFolderPathOfFolder;
            folder.ParentFolder = newParent!;

            Folder? finalMainFolder = await _foldersRepository.UpdateFolder(folder, true, true, false, false, false, false);

            List<Folder> tempTraversal = new List<Folder>() { folder };
            while (tempTraversal.Count>0)
            {
                Folder temp = tempTraversal.ElementAt(0);
                if (temp.FolderId != folder.FolderId) 
                {
                    string folderPathBeforeAfter = temp.FolderPath;
                    folderPathBeforeAfter = folderPathBeforeAfter.Replace(previousFolderPath, newFolderPathOfFolder);
                    temp.FolderPath = folderPathBeforeAfter;
                    await _foldersRepository.UpdateFolder(temp, true, false, false, false, false, false);
                }
                if (temp.SubFolders.Count > 0) 
                {
                    foreach (Folder temp2 in temp.SubFolders) 
                    {
                        tempTraversal.Add(temp2);
                    }
                }
                foreach (Domain.Entities.File temp2 in temp.Files) 
                {
                    string filePathBeforeAfter = temp2.FilePath;
                    filePathBeforeAfter = filePathBeforeAfter.Replace(previousFolderPath, newFolderPathOfFolder);
                    temp2.FilePath = filePathBeforeAfter;
                    await _filesRepository.UpdateFile(temp2, true, false, false, false);
                }
                tempTraversal.RemoveAt(0);
            }
            return finalMainFolder!.ToFolderResponse();
        }

        public async Task<FolderResponse> RenameFolder(FolderRenameRequest folderRenameRequest)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(folderRenameRequest.FolderId);
            if (folder == null) 
            {
                throw new ArgumentException();
            }
            if (Directory.Exists(folder.FolderPath.Replace(folder.FolderName, folderRenameRequest.FolderNewName))) 
            {
                throw new InvalidOperationException();
            }
            folder.FolderName = folderRenameRequest.FolderNewName;
            Folder? updatedFolder = await _foldersRepository.UpdateFolder(folder, true, false, false, false, false, false);
            // if a directory doesnt exists in specified folder it will rename instead of moving
            Directory.Move(folder.FolderPath, folder.FolderPath.Replace(folder.FolderName, folderRenameRequest.FolderNewName));
            return updatedFolder!.ToFolderResponse();
        }
    }
}
