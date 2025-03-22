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

        public async Task<List<FolderResponse>> GetAllFoldersInHomeFolder(SortOrderOptions sortOptions)
        {
            Folder? homeFolder = await _foldersRepository.GetFolderByFolderPath(Path.Combine(_configuration["InitialPathForStorage"], "home"));
            if (homeFolder == null) 
            {
                return new List<FolderResponse>();
            }
            List<Folder> homeFolders = homeFolder!.SubFolders.ToList();
            if (homeFolders.Count <= 0) 
            {
                return new List<FolderResponse>();
            }
            List<Folder> sorted = Utilities.Sort(homeFolders, sortOptions); 
            List<FolderResponse> folderResponses = sorted.Select(f=>f.ToFolderResponse()).ToList();
            return folderResponses;
        }

        public async Task<List<FolderResponse>> GetAllSubFolders(Guid parentFolderId, SortOrderOptions sortOptions)
        {
            Folder? parentFolder = await _foldersRepository.GetFolderByFolderId(parentFolderId);
            if (parentFolder == null)
            {
                throw new ArgumentException();
            }
            List<Folder> childFolders = parentFolder!.SubFolders.ToList();

            await Utilities.UpdateMetadataOpen(parentFolder, _foldersRepository);
            if (childFolders.Count <= 0)
            {
                return new List<FolderResponse>();
            }
            List<Folder> sorted = Utilities.Sort(childFolders, sortOptions);
            List<FolderResponse> folderResponses = sorted.Select(f => f.ToFolderResponse()).ToList();
            return folderResponses;
        }

        public async Task<List<FolderResponse>> GetFilteredFolders(string searchString, SortOrderOptions sortOptions)
        {
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f=>f.FolderName.Contains(searchString));
            if (folders.Count <= 0) 
            {
                return new List<FolderResponse>();
            }
            List<Folder> sortedFolderzzzzzz = Utilities.Sort(folders, sortOptions);
            return sortedFolderzzzzzz.Select(f => f.ToFolderResponse()).ToList();
        }

        public async Task<List<FolderResponse>> GetAllFavoriteFolders(SortOrderOptions sortOptions)
        {
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f => f.IsFavorite);
            if (folders.Count <= 0)
            {
                return new List<FolderResponse>();
            }
            List<Folder> sortedFolderzzzzzz = Utilities.Sort(folders, sortOptions);
            return sortedFolderzzzzzz.Select(f => f.ToFolderResponse()).ToList();
        } //TODO Unit test this service

        public async Task<List<FolderResponse>> GetAllTrashFolders(SortOrderOptions sortOptions)
        {
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f => f.IsTrash);
            if (folders.Count <= 0)
            {
                return new List<FolderResponse>();
            }
            List<Folder> sortedFolders = Utilities.Sort(folders, sortOptions);
            return sortedFolders.Select(f => f.ToFolderResponse()).ToList();
        } //TODO Unit test this service

        public async Task<FolderResponse?> GetFolderByFolderId(Guid fid)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(fid);
            if (folder == null) 
            {
                return null;
            }
            return folder.ToFolderResponse();
        }

        public async Task<FolderResponse?> GetFolderByFolderPath(string fpath)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderPath(fpath);
            if (folder == null)
            {
                return null;
            }
            return folder.ToFolderResponse();
        }

        public async Task<FileOrFolderMetadataResponse> GetMetadata(Guid fid)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(fid);
            if (folder == null)
            {
                throw new ArgumentException();
            }
            FileOrFolderMetadataResponse response = folder!.Metadata!.ToMetadataResponse();
            response.CreationDate = folder.CreationDate;
            response.SubFilesCount = folder.Files.Count;
            response.SubFoldersCount = folder.SubFolders.Count;
            response.ParentFolderName = folder.ParentFolder!.FolderName;
            return response;
        }
    }
}
