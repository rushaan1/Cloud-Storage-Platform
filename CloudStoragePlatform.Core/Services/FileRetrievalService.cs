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

        public async Task<FileResponse?> GetFileByFileId(Guid id)
        {
            var file = await _filesRepository.GetFileByFileId(id);
            if (file == null)
            {
                return null;
            }
            return file.ToFileResponse();
        }

        public async Task<FileResponse?> GetFileByFilePath(string path)
        {
            var file = await _filesRepository.GetFileByFilePath(path);
            if (file == null)
            {
                return null;
            }
            return file.ToFileResponse();
        }

        public async Task<FileOrFolderMetadataResponse> GetMetadata(Guid fileId)
        {
            var file = await _filesRepository.GetFileByFileId(fileId);
            if (file == null)
            {
                throw new ArgumentException();
            }
            var response = file!.Metadata!.ToMetadataResponse();
            response.CreationDate = file.CreationDate;
            response.ParentFolderName = file.ParentFolder.FolderName;
            return response;
        }
    }
}
