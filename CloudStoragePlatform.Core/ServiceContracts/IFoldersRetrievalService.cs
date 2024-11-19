using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.ServiceContracts
{
    public interface IFoldersRetrievalService
    {
        Task<FolderResponse> GetFolderByFolderId(Guid id);
        Task<FolderResponse> GetFolderByFolderPath(string path);
        Task<List<FolderResponse>> GetAllFoldersInHomeFolder(SortOrderOptions sortOptions);
        Task<List<FolderResponse>> GetAllSubFolders(Guid parentFolderId, SortOrderOptions sortOptions);
        Task<List<FolderResponse>> GetFilteredFolders(string searchString, SortOrderOptions sortOptions);
        Task<MetadataResponse> GetMetadata(Guid folderId);
    }
}
