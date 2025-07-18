﻿using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.ServiceContracts
{
    public interface IBulkRetrievalService
    {

        Task DownloadFolder(List<Guid> folderIds, List<Guid> fileIds, Stream outputStream);
        Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllChildren(Guid parentFolderId, SortOrderOptions sortOptions);
        Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllFavorites(SortOrderOptions sortOptions);
        Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllInHome(SortOrderOptions sortOptions);
        Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllTrashes(SortOrderOptions sortOptions);
        Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllFilteredChildren(string searchString, SortOrderOptions sortOptions);
        Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllRecents();
        Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllMediaFiles(SortOrderOptions sortOptions);
        Task<UsageAnalyticsResult> GetUsageAnalytics();
    }
}
