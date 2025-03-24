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
using File = CloudStoragePlatform.Core.Domain.Entities.File;

namespace CloudStoragePlatform.Core.Services
{
    public class RetrievalService : IRetrievalService
    {
        private readonly IFoldersRepository _foldersRepository;
        private readonly IFilesRepository _filesRepository;
        private readonly IConfiguration _configuration;
        public RetrievalService(IFoldersRepository foldersRepository, IConfiguration configuration, IFilesRepository filesRepository)
        {
            _foldersRepository = foldersRepository;
            _configuration = configuration;
            _filesRepository = filesRepository;
        }

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllInHome(SortOrderOptions sortOptions, bool retrieveFiles)
        {
            Folder? homeFolder = await _foldersRepository.GetFolderByFolderPath(Path.Combine(_configuration["InitialPathForStorage"], "home"));
            if (homeFolder == null)
            {
                return (new List<FolderResponse>(), new List<FileResponse>());
            }
            List<Folder> foldersInHome = homeFolder!.SubFolders.ToList();
            if (foldersInHome.Count <= 0)
            {
                return (new List<FolderResponse>(), new List<FileResponse>());
            }

            if (retrieveFiles)
            {
                return GetResponse(foldersInHome, homeFolder!.Files.ToList(), sortOptions);
            }
            else 
            {
                return GetResponse(foldersInHome, sortOptions);
            }
        }

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllChildren(Guid parentFolderId, SortOrderOptions sortOptions, bool retrieveFiles)
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
                return (new List<FolderResponse>(), new List<FileResponse>());
            }
            if (retrieveFiles)
            {
                return GetResponse(childFolders, parentFolder!.Files.ToList(), sortOptions);
            }
            else
            {
                return GetResponse(childFolders, sortOptions);
            }
        }

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllFilteredChildren(string searchString, SortOrderOptions sortOptions, bool retrieveFiles)
        {
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f => f.FolderName.Contains(searchString));
            
            if (folders.Count <= 0)
            {
                return (new List<FolderResponse>(), new List<FileResponse>());
            }
            if (retrieveFiles)
            {
                List<File> files = await _filesRepository.GetFilteredFiles(f => f.FileName.Contains(searchString));
                return GetResponse(folders, files, sortOptions);
            }
            else
            {
                return GetResponse(folders, sortOptions);
            }
        }

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllFavorites(SortOrderOptions sortOptions, bool retrieveFiles)
        {
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f => f.IsFavorite);
            if (folders.Count <= 0)
            {
                return (new List<FolderResponse>(), new List<FileResponse>());
            }
            if (retrieveFiles)
            {
                List<File> files = await _filesRepository.GetFilteredFiles(f => f.IsFavorite);
                return GetResponse(folders, files, sortOptions);
            }
            else
            {
                return GetResponse(folders, sortOptions);
            }
        } //TODO Unit test this service

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllTrashes(SortOrderOptions sortOptions, bool retrieveFiles)
        {
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f => f.IsTrash);
            
            if (folders.Count <= 0)
            {
                return (new List<FolderResponse>(), new List<FileResponse>());
            }

            if (retrieveFiles)
            {
                List<File> files = await _filesRepository.GetFilteredFiles(f => f.IsFavorite);
                return GetResponse(folders, files, sortOptions);
            }
            else
            {
                return GetResponse(folders, sortOptions);
            }
        } //TODO Unit test this service

        private (List<FolderResponse> Folders, List<FileResponse> Files) GetResponse(List<Folder> folders, List<File> files, SortOrderOptions sortOptions)
        {
            List<Folder> sortedFolders = Utilities.Sort(folders, sortOptions);
            List<FolderResponse> folderResponses = sortedFolders.Select(f => f.ToFolderResponse()).ToList();

            List<File> sortedFiles = Utilities.Sort(files, sortOptions).ToList();
            List<FileResponse> sortedFileResponses = sortedFiles.Select(f => f.ToFileResponse()).ToList();
            return (folderResponses, sortedFileResponses);
        }

        private (List<FolderResponse> Folders, List<FileResponse> Files) GetResponse(List<Folder> folders, SortOrderOptions sortOptions)
        {
            List<Folder> sortedFolders = Utilities.Sort(folders, sortOptions);
            List<FolderResponse> folderResponses = sortedFolders.Select(f => f.ToFolderResponse()).ToList();
            return (folderResponses, new List<FileResponse>());
        }
    }
}
