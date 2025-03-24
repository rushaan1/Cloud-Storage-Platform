using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.ServiceContracts
{
    public interface IFilesRetrievalService
    {
        Task<FileResponse?> GetFileByFileId(Guid id);
        Task<FileResponse?> GetFileByFilePath(string path);
        Task<List<FileResponse>> GetAllFilesInHome(SortOrderOptions sortOptions);
        Task<List<FileResponse>> GetFilteredFiles(string searchString, SortOrderOptions sortOptions);
        Task<FileOrFolderMetadataResponse> GetMetadata(Guid fileId);
        Task<List<FileResponse>> GetAllFavoriteFiles(SortOrderOptions sortOptions);
        Task<List<FileResponse>> GetAllTrashFiles(SortOrderOptions sortOptions);
    }
}
