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
        public FoldersModificationService(IFoldersRepository foldersRepository) 
        {
            _foldersRepository = foldersRepository;
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
            throw new NotImplementedException();
        }

        public async Task<FolderResponse> RenameFolder(FolderRenameRequest folderRenameRequest)
        {
            throw new NotImplementedException();
        }
    }
}
