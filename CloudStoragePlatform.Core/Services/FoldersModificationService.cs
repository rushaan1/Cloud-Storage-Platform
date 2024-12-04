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
            Folder? folder = null;
            if (Directory.Exists(ReplaceLastOccurance(folderAddRequest.FolderPath, folderAddRequest.FolderName, "")))
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

                folder = new Folder() { FolderId = Guid.NewGuid(), FolderName = folderAddRequest.FolderName, FolderPath = folderAddRequest.FolderPath, Metadata = metadata, Sharing = sharing };
                metadata.Folder = folder;
                sharing.Folder = folder;
                await _foldersRepository.AddFolder(folder);
                Directory.CreateDirectory(folderAddRequest.FolderPath);
            }
            else 
            {
                throw new ArgumentException();
            }
            return folder.ToFolderResponse();
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
            Directory.Delete(folder.FolderPath);
            return await _foldersRepository.DeleteFolder(folder);
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

            if (Directory.Exists(newFolderPathOfFolder))
            {
                throw new DuplicateFolderException();
            }
            
            Folder? newParent = await _foldersRepository.GetFolderByFolderPath(newFolderPath);
            folder.FolderPath = newFolderPathOfFolder;
            folder.ParentFolder = newParent!;

            Folder? finalMainFolder = await _foldersRepository.UpdateFolder(folder, true, true, false, false, false, false);
            await UpdateChildPaths(folder, previousFolderPath, newFolderPath);
            Directory.Move(previousFolderPath, newFolderPathOfFolder);

            return finalMainFolder!.ToFolderResponse();
        }

        public async Task<FolderResponse> RenameFolder(FolderRenameRequest folderRenameRequest)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(folderRenameRequest.FolderId);
            if (folder == null) 
            {
                throw new ArgumentException();
            }
            string newp = ReplaceLastOccurance(folder.FolderPath, folder.FolderName, folderRenameRequest.FolderNewName);
            if (Directory.Exists(newp)) 
            {
                throw new DuplicateFolderException();
            }
            string oldp = folder!.FolderPath;
            folder.FolderName = folderRenameRequest.FolderNewName;
            folder.FolderPath = newp;
            await UpdateChildPaths(folder, oldp, newp);
            Folder? updatedFolder = await _foldersRepository.UpdateFolder(folder, true, false, false, false, false, false);
            FileSystem.RenameDirectory(oldp, folderRenameRequest.FolderNewName);
            return updatedFolder!.ToFolderResponse();
        }





        // Utility Functions
        private async Task UpdateChildPaths(Folder source, string oldp, string newp)
        {
            Queue<Folder> tempTraversal = new Queue<Folder>();
            tempTraversal.Enqueue(source);
            while (tempTraversal.Count > 0)
            {
                Folder temp = tempTraversal.Dequeue();
                if (temp.FolderId != source.FolderId)
                {
                    string folderPathBeforeAfter = temp.FolderPath;
                    folderPathBeforeAfter = folderPathBeforeAfter.Replace(oldp, newp);
                    temp.FolderPath = folderPathBeforeAfter;
                    await _foldersRepository.UpdateFolder(temp, true, false, false, false, false, false);
                }
                if (temp.SubFolders.Count > 0)
                {
                    foreach (Folder temp2 in temp.SubFolders)
                    {
                        tempTraversal.Enqueue(temp2);
                    }
                }
                foreach (Domain.Entities.File temp2 in temp.Files)
                {
                    string filePathBeforeAfter = temp2.FilePath;
                    filePathBeforeAfter = filePathBeforeAfter.Replace(oldp, newp);
                    temp2.FilePath = filePathBeforeAfter;
                    await _filesRepository.UpdateFile(temp2, true, false, false, false);
                }
            }
        }
        //Using this function instead of normally replacing to ensure only that specific folder name is replaced because .Replace() will replace all occurances so it may replace an occurance which isn't the folder's name
        private string ReplaceLastOccurance(string main, string previousPart, string newPart)
        {
            int lastIndex = main.LastIndexOf(previousPart);
            string replacedString = main.Substring(0, lastIndex) + newPart;
            return replacedString;
        }
    }
}
