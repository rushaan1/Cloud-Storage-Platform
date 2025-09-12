using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.IdentityEntites;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.ServiceContracts;
using System;
using System.Threading.Tasks;
using File = CloudStoragePlatform.Core.Domain.Entities.File;

namespace CloudStoragePlatform.Core.Services
{
    public class SharingService : ISharingService
    {
        private readonly IFilesRepository _filesRepository;
        private readonly IFoldersRepository _foldersRepository;
        private readonly ISharingRepository _sharingRepository;
        private readonly UserIdentification _userIdentification;

        public SharingService(IFilesRepository filesRepository, IFoldersRepository foldersRepository, ISharingRepository sharingRepository, UserIdentification userIdentification)
        {
            _filesRepository = filesRepository;
            _foldersRepository = foldersRepository;
            _sharingRepository = sharingRepository;
            _userIdentification = userIdentification;
        }

        public async Task<Guid?> CreateShareForFile(Guid fileId, DateTime? expiry = null)
        {
            var file = await _filesRepository.GetFileByFileId(fileId);
            if (file == null)
            {
                return null;
            }

            var sharing = new Sharing
            {
                SharingId = Guid.NewGuid(),
                File = file,
                UserId = file.UserId,
                ShareLinkUrl = null,
                ShareLinkExpiry = expiry,
                ShareLinkCreateDate = DateTime.UtcNow
            };

            await _sharingRepository.CreateSharing(sharing);

            file.SharingId = sharing.SharingId;
            file.Sharing = sharing;
            await _filesRepository.UpdateFile(file, true, false, false, true);
            return sharing.SharingId;
        }

        public async Task<Guid?> CreateShareForFolder(Guid folderId, DateTime? expiry = null)
        {
            var folder = await _foldersRepository.GetFolderByFolderId(folderId);
            if (folder == null)
            {
                return null;
            }

            var sharing = new Sharing
            {
                SharingId = Guid.NewGuid(),
                Folder = folder,
                UserId = folder.UserId,
                ShareLinkUrl = null,
                ShareLinkExpiry = expiry,
                ShareLinkCreateDate = DateTime.UtcNow
            };

            await _sharingRepository.CreateSharing(sharing);

            folder.SharingId = sharing.SharingId;
            folder.Sharing = sharing;
            await _foldersRepository.UpdateFolder(folder, true, false, false, true, false, false);
            return sharing.SharingId;
        }

        public async Task<bool> RemoveShareForFile(Guid fileId)
        {
            var file = await _filesRepository.GetFileByFileId(fileId);
            if (file == null || file.Sharing == null)
            {
                return false;
            }

            var sharing = file.Sharing;
            file.Sharing = null;
            file.SharingId = null;
            await _filesRepository.UpdateFile(file, true, false, false, true);
            return await _sharingRepository.DeleteSharing(sharing);
        }

        public async Task<bool> RemoveShareForFolder(Guid folderId)
        {
            var folder = await _foldersRepository.GetFolderByFolderId(folderId);
            if (folder == null || folder.Sharing == null)
            {
                return false;
            }

            var sharing = folder.Sharing;
            folder.Sharing = null;
            folder.SharingId = null;
            await _foldersRepository.UpdateFolder(folder, true, false, false, true, false, false);
            return await _sharingRepository.DeleteSharing(sharing);
        }

        public async Task<(File? file, Folder? folder, bool childFile, string relativeSubjectPath)?> ValidateShareFetchSubject(Guid sharingId, Guid fileFolderSubjectId) 
        { // subject is essentially the file/folder being fetched, it may be the shared file itself or a shared folder itself or child of a shared folder
            Sharing? share = await _sharingRepository.GetSharingById(sharingId);
            if (share == null || (share.ShareLinkExpiry.HasValue && share.ShareLinkExpiry.Value < DateTime.UtcNow))
            {
                return null;
            }
            // Fetching share & validating expiry ^ 

            ApplicationUser? sharedByUser = share.User;
            if (sharedByUser == null) { return null; }
            _userIdentification.User = sharedByUser;

            // get user set user ^

            File? sharedFile = share.File;
            if (sharedFile != null) 
            {
                if (fileFolderSubjectId == sharedFile.FileId) 
                {
                    return (sharedFile, null, false, sharedFile.FileName);
                }
            } // in case its a shared file only ^

            Folder? sharedFolder = share.Folder;
            if (sharedFolder != null) 
            {
                if (sharedFolder.FolderId == fileFolderSubjectId)
                {
                    return (null, sharedFolder, false, sharedFolder.FolderName);
                } // in case its a shared folder & subject is shared folder too ^

                else 
                {
                    File? file = await _filesRepository.GetFileByFileId(fileFolderSubjectId);
                    if (file != null) 
                    {
                        if (file.FilePath.Contains(sharedFolder.FolderPath))
                        {
                            return (file, null, true, sharedFolder.FolderName + file.FilePath.Replace(sharedFolder.FolderPath,""));
                        } // \ is added on its own as a result of path replacing 
                        else { return null; }
                    } // in case its a child file of shared folder ^
                    Folder? folder = await _foldersRepository.GetFolderByFolderId(fileFolderSubjectId);
                    if (folder != null) 
                    {
                        if (folder.FolderPath.Contains(sharedFolder.FolderPath)) 
                        {
                            return (null, folder, false, sharedFolder.FolderName + folder.FolderPath.Replace(sharedFolder.FolderPath, ""));
                        }
                    } // in case its a child folder of shared folder ^
                }
            }
            return null;
        }

        public async Task<Folder?> FetchPublicFolder(Guid sharingId, string relativePath)
        {
            Sharing? share = await _sharingRepository.GetSharingById(sharingId);
            if (share == null || (share.ShareLinkExpiry.HasValue && share.ShareLinkExpiry.Value < DateTime.UtcNow))
            {
                return null;
            }

            // Increment visits BEFORE switching user context
            await _sharingRepository.IncrementVisits(sharingId);

            ApplicationUser? sharedByUser = share.User;
            if (sharedByUser == null) { return null; }
            _userIdentification.User = sharedByUser;

            // Switch user context

            Folder? sharedFolder = share.Folder;
            if (sharedFolder == null)
            {
                return null;
            }

            // If path is same as shared folder's path, return the shared folder
            if (string.IsNullOrEmpty(relativePath) || relativePath == sharedFolder.FolderPath)
            {
                return sharedFolder;
            }

            // Split relative path by \ and remove first element
            string[] pathParts = relativePath.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length == 0)
            {
                return sharedFolder;
            }

            // Remove first element and combine with shared folder path
            string mutatedPath = string.Join("\\", pathParts.Skip(1));
            string combinedPath = Path.Combine(sharedFolder.FolderPath, mutatedPath);

            // Find and return the matching folder
            return await _foldersRepository.GetFolderByFolderPath(combinedPath);
        }
    }
}


