﻿using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
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
        public Task<FolderResponse> AddFolder(FolderAddRequest folderAddRequest)
        {
            throw new NotImplementedException();
        }

        public Task<FolderResponse> AddOrRemoveFavorite(Guid folderId)
        {
            throw new NotImplementedException();
        }

        public Task<FolderResponse> AddOrRemoveTrash(Guid folderId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteFolder(Guid folderId)
        {
            throw new NotImplementedException();
        }

        public Task<FolderResponse> MoveFolder(Guid folderId, string newFolderPath)
        {
            throw new NotImplementedException();
        }

        public Task<FolderResponse> RenameFolder(FolderRenameRequest folderRenameRequest)
        {
            throw new NotImplementedException();
        }
    }
}