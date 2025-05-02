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
        Task<FileStream> GetFilePreview(string filePath);
        Task<FileResponse?> GetFileByFilePath(string path);
        Task<FileOrFolderMetadataResponse> GetMetadata(Guid fileId);
    }
}
