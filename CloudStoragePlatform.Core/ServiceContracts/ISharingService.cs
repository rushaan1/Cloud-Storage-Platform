using CloudStoragePlatform.Core.Domain.Entities;
using System;
using System.Threading.Tasks;
using File = CloudStoragePlatform.Core.Domain.Entities.File;

namespace CloudStoragePlatform.Core.ServiceContracts
{
    public interface ISharingService
    {
        Task<Guid?> CreateShareForFile(Guid fileId, DateTime? expiry = null);
        Task<Guid?> CreateShareForFolder(Guid folderId, DateTime? expiry = null);

        Task<bool> RemoveShareForFile(Guid fileId);
        Task<bool> RemoveShareForFolder(Guid folderId);

        Task<(File? file, Folder? folder, bool childFile, string relativeSubjectPath)?> ValidateShareFetchSubject(Guid sharingId, Guid fileFolderSubjectId);

        Task<Folder?> FetchPublicFolder(Guid sharingId, string relativePath);
    }
}


