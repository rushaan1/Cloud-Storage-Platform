﻿using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.ServiceContracts
{
    public interface IFolderRetrievalService
    {
        Task<FolderResponse> GetFolderByFolderId(Guid id);
        Task<FolderResponse> GetFolderByFolderPath(string path);
        Task<List<FolderResponse>> GetAllFoldersInHomeFolder(SortOrderOptions sortOptions);
        Task<List<FolderResponse>> GetAllSubFolders(Guid parentFolderId, SortOrderOptions sortOptions);
        Task<List<FolderResponse>> GetFilteredFolders(string searchString);
        Task<MetadataResponse> GetMetadata(Guid folderId);
    }
}