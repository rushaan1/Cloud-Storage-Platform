using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using CloudStoragePlatform.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Services
{
    public class FileRetrievalService : IFilesRetrievalService
    {
        private readonly IFilesRepository _filesRepository;
        private readonly IConfiguration _configuration;
        public FileRetrievalService(IFilesRepository filesRepository, IConfiguration configuration)
        {
            _filesRepository = filesRepository;
            _configuration = configuration;
        }

        public Task<List<FileResponse>> GetAllFavoriteFiles(SortOrderOptions sortOptions)
        {
            throw new NotImplementedException();
        }

        public Task<List<FileResponse>> GetAllFilesInHome(SortOrderOptions sortOptions)
        {
            throw new NotImplementedException();
        }

        public Task<List<FileResponse>> GetAllTrashFiles(SortOrderOptions sortOptions)
        {
            throw new NotImplementedException();
        }

        public Task<FileResponse?> GetFileByFileId(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<FileResponse?> GetFileByFilePath(string path)
        {
            throw new NotImplementedException();
        }

        public Task<List<FileResponse>> GetFilteredFiles(string searchString, SortOrderOptions sortOptions)
        {
            throw new NotImplementedException();
        }

        public Task<FileOrFolderMetadataResponse> GetMetadata(Guid fileId)
        {
            throw new NotImplementedException();
        }
    }
}
