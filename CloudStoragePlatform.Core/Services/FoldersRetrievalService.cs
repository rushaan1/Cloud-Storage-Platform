using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using CloudStoragePlatform.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Services
{
    public class FoldersRetrievalService : IFoldersRetrievalService
    {
        private readonly IFoldersRepository _foldersRepository;
        public FoldersRetrievalService(IFoldersRepository foldersRepository)
        {
            _foldersRepository = foldersRepository;
        }


        public Task<List<FolderResponse>> GetAllFoldersInHomeFolder(SortOrderOptions sortOptions)
        {
            throw new NotImplementedException();
        }

        public Task<List<FolderResponse>> GetAllSubFolders(Guid parentFolderId, SortOrderOptions sortOptions)
        {
            throw new NotImplementedException();
        }

        public Task<List<FolderResponse>> GetFilteredFolders(string searchString)
        {
            throw new NotImplementedException();
        }

        public Task<FolderResponse> GetFolderByFolderId(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<FolderResponse> GetFolderByFolderPath(string path)
        {
            throw new NotImplementedException();
        }

        public Task<MetadataResponse> GetMetadata(Guid folderId)
        {
            throw new NotImplementedException();
        }
    }
}
