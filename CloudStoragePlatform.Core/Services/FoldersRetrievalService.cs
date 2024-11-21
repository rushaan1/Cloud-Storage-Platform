using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using CloudStoragePlatform.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Services
{
    public class FoldersRetrievalService : IFoldersRetrievalService
    {
        private readonly IFoldersRepository _foldersRepository;
        private readonly IConfiguration _configuration;
        public FoldersRetrievalService(IFoldersRepository foldersRepository, IConfiguration configuration) 
        {
            _foldersRepository = foldersRepository;
            _configuration = configuration;
        }

        public async Task<List<Folder>> Sort(SortOrderOptions option)
        {
            throw new NotImplementedException();
        } // UTILITY FUNCTION

        public async Task<List<FolderResponse>> GetAllFoldersInHomeFolder(SortOrderOptions sortOptions)
        {
            Folder? homeFolder = await _foldersRepository.GetFolderByFolderPath(_configuration["InitialPathForStorage"]);
            List<Folder> homeFolders = homeFolder!.SubFolders.ToList();
            if (homeFolders.Count <= 0) 
            {

            }
        }

        public Task<List<FolderResponse>> GetAllSubFolders(Guid parentFolderId, SortOrderOptions sortOptions)
        {
            throw new NotImplementedException();
        }

        public Task<List<FolderResponse>> GetFilteredFolders(string searchString, SortOrderOptions sortOptions)
        {
            throw new NotImplementedException();
        }

        public async Task<FolderResponse> GetFolderByFolderId(Guid fid)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(fid);
            if (folder == null) 
            {
                throw new ArgumentException();
            }
            return folder.ToFolderResponse();
        }

        public async Task<FolderResponse> GetFolderByFolderPath(string fpath)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderPath(fpath);
            if (folder == null)
            {
                throw new ArgumentException();
            }
            return folder.ToFolderResponse();
        }

        public Task<MetadataResponse> GetMetadata(Guid folderId)
        {
            throw new NotImplementedException();
        }
    }
}
