using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using CloudStoragePlatform.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using File = CloudStoragePlatform.Core.Domain.Entities.File;

namespace CloudStoragePlatform.Core.Services
{
    public class BulkRetrievalService : IBulkRetrievalService
    {
        private readonly IFoldersRepository _foldersRepository;
        private readonly IFilesRepository _filesRepository;
        private readonly IConfiguration _configuration;
        private readonly UserIdentification _userIdentification;
        private readonly ThumbnailService _thumbnailService;
        public BulkRetrievalService(IFoldersRepository foldersRepository, IConfiguration configuration, IFilesRepository filesRepository, UserIdentification userIdentification, ThumbnailService thumbnailService)
        {
            _foldersRepository = foldersRepository;
            _configuration = configuration;
            _filesRepository = filesRepository;
            _userIdentification = userIdentification;
            _thumbnailService = thumbnailService;
        }

        public async Task DownloadFolder(List<Guid> folderIds, List<Guid> fileIds, Stream outputStream)
        {
            List<Folder?> folders = new();
            List<File?> files = new();
            foreach (var fid in folderIds)
            {
                folders.Add(await _foldersRepository.GetFolderByFolderId(fid));
            }
            foreach (var fid in fileIds)
            {
                files.Add(await _filesRepository.GetFileByFileId(fid));
            }

            if (folders.Contains(null) || files.Contains(null))
            {
                throw new DirectoryNotFoundException();
            }

            if (folders.Count == 0 && files.Count == 1)
            {
                var file = files.First();
                if (file != null)
                {
                    await using var fileStream = System.IO.File.OpenRead(Path.Combine(_userIdentification.PhysicalStoragePath, file.FileId.ToString()));
                    await fileStream.CopyToAsync(outputStream);
                }
                return;
            }

            using (ZipArchive archive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: false))
            {
                foreach (var folder in folders)
                {
                    List<File> allSubFilesUptoMaxDepth = await _filesRepository.GetFilteredFiles(f => (f.FilePath + "\\").Contains(folder.FolderPath + "\\"));
                    List<Folder> allSubFoldersUptoMaxDepth = await _foldersRepository.GetFilteredFolders(f => (f.FolderPath + "\\").Contains(folder.FolderPath + "\\"));

                    foreach (var subFolder in allSubFoldersUptoMaxDepth)
                    {
                        string path = $"{folder.FolderName}" + subFolder.FolderPath.Replace(folder.FolderPath, "").Replace("\\", "/") + "/";
                        if (folders.Count > 1)
                        {
                            path = folders.Count + " folders inside/" + path;
                        }
                        archive.CreateEntry(path);
                    }
                    foreach (File subFile in allSubFilesUptoMaxDepth)
                    {
                        string path = $"{folder.FolderName}" + subFile.FilePath.Replace(folder.FolderPath, "").Replace("\\", "/");
                        if (folders.Count > 1)
                        {
                            path = folders.Count + " folders inside/" + path;
                        }
                        var entry = archive.CreateEntry(path);
                        using var entryStream = entry.Open();
                        await using var fileStream = System.IO.File.OpenRead(Path.Combine(_userIdentification.PhysicalStoragePath, subFile.FileId.ToString()));
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
                foreach (var file in files)
                {
                    var entry = archive.CreateEntry(file.FileName);
                    using var entryStream = entry.Open();
                    await using var fileStream = System.IO.File.OpenRead(Path.Combine(_userIdentification.PhysicalStoragePath, file.FileId.ToString()));
                    await fileStream.CopyToAsync(entryStream);
                }
            }
        }

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllInHome(SortOrderOptions sortOptions)
        {
            Folder? homeFolder = await _foldersRepository.GetFolderByFolderPath(Path.Combine(_configuration["InitialPathForStorage"], "home"));
            if (homeFolder == null)
            {
                return (new List<FolderResponse>(), new List<FileResponse>());
            }
            List<Folder> foldersInHome = homeFolder!.SubFolders.ToList();

            return GetResponse(foldersInHome, homeFolder!.Files.ToList(), sortOptions);
        }

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllChildren(Guid parentFolderId, SortOrderOptions sortOptions)
        {
            Folder? parentFolder = await _foldersRepository.GetFolderByFolderId(parentFolderId);
            if (parentFolder == null)
            {
                throw new ArgumentException();
            }
            List<Folder> childFolders = parentFolder!.SubFolders.ToList();

            await Utilities.UpdateMetadataOpen(parentFolder, _foldersRepository);
            return GetResponse(childFolders, parentFolder!.Files.ToList(), sortOptions);
        }

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllFilteredChildren(string searchString, SortOrderOptions sortOptions)
        {
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f => f.FolderName.ToLower().Contains(searchString.ToLower()));

            List<File> files = await _filesRepository.GetFilteredFiles(f => f.FileName.ToLower().Contains(searchString.ToLower()));
            return GetResponse(folders, files, sortOptions);
        }

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllFavorites(SortOrderOptions sortOptions)
        {
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f => f.IsFavorite);

            List<File> files = await _filesRepository.GetFilteredFiles(f => f.IsFavorite);
            return GetResponse(folders, files, sortOptions);
        }

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllTrashes(SortOrderOptions sortOptions)
        {
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f => f.IsTrash);


            List<File> files = await _filesRepository.GetFilteredFiles(f => f.IsTrash);
            return GetResponse(folders, files, sortOptions);
        }

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllRecents()
        {
            List<Folder> folders = await _foldersRepository.GetAllFolders();
            folders = folders.Where(f => f.Metadata!=null && f.Metadata.LastOpened != null).OrderByDescending(f => f.Metadata.LastOpened).Take(15).ToList();
            
            List<File> files = await _filesRepository.GetAllFiles();
            files = files.Where(f => f.Metadata.LastOpened != null).OrderByDescending(f => f.Metadata.LastOpened).Take(15).ToList();

            return GetResponse(folders, files, SortOrderOptions.LASTOPENED_DESCENDING);
        }

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllMediaFiles(SortOrderOptions sortOptions)
        {
            // Get all files that don't have Document file type
            List<File> files = await _filesRepository.GetFilteredFiles(f => f.FileType != FileType.Document);
            
            // We return an empty list of folders since we're only interested in non-document files
            return GetResponse(new List<Folder>(), files, sortOptions);
        }

        public async Task<UsageAnalyticsResult> GetUsageAnalytics()
        {
            var files = await _filesRepository.GetAllFiles();
            var folders = await _foldersRepository.GetAllFolders();

            // Group by extension and sum sizes
            List<ExtensionSizeSummary> extensionGroups = files
                .GroupBy(f => Path.GetExtension(f.FileName)?.ToLower() ?? "")
                .Select(g => new ExtensionSizeSummary
                {
                    Extension = string.IsNullOrWhiteSpace(g.Key) ? "no extension" : g.Key,
                    TotalSize = g.Sum(f => f.Size)
                })
                .OrderByDescending(e => e.TotalSize)
                .Take(10)
                .ToList();

            // Top 10 files by size
            List<FileSizeSummary> topFiles = files
                .OrderByDescending(f => f.Size)
                .Take(10)
                .Select(f => new FileSizeSummary
                {
                    FileName = f.FileName,
                    Size = f.Size
                })
                .ToList();

            // Total counts
            int totalFolders = folders.Count;
            int totalFiles = files.Count;

            // Favorite items (files + folders)
            int favoriteItems = files.Count(f => f.IsFavorite) + folders.Count(f => f.IsFavorite);

            // Shared items (files + folders)
            int itemsShared = 0;
            DateTime now = DateTime.UtcNow;
            // Files
            itemsShared += files.Count(f =>
                f.Sharing != null &&
                !string.IsNullOrWhiteSpace(f.Sharing.ShareLinkUrl) &&
                (
                    !f.Sharing.ShareLinkExpiry.HasValue ||
                    f.Sharing.ShareLinkExpiry.Value > now
                )
            );
            // Folders
            itemsShared += folders.Count(f =>
                f.Sharing != null &&
                !string.IsNullOrWhiteSpace(f.Sharing.ShareLinkUrl) &&
                (
                    !f.Sharing.ShareLinkExpiry.HasValue ||
                    f.Sharing.ShareLinkExpiry.Value > now
                )
            );

            return new UsageAnalyticsResult
            {
                TopExtensionsBySize = extensionGroups,
                TopFilesBySize = topFiles,
                TotalFolders = totalFolders,
                TotalFiles = totalFiles,
                FavoriteItems = favoriteItems,
                ItemsShared = itemsShared
            };
        }

        private (List<FolderResponse> Folders, List<FileResponse> Files) GetResponse(List<Folder> folders, List<File> files, SortOrderOptions sortOptions)
        {
            // Exclude the user's home folder from the output
            string homeFolderPath = Path.Combine(_configuration["InitialPathForStorage"], "home");
            List<Folder> filteredFolders = folders.Where(f => !string.Equals(f.FolderPath, homeFolderPath, StringComparison.OrdinalIgnoreCase)).ToList();
            List<Folder> sortedFolders = Utilities.Sort(filteredFolders, sortOptions);
            List<FolderResponse> folderResponses = sortedFolders.Select(f => f.ToFolderResponse()).ToList();

            List<File> sortedFiles = Utilities.Sort(files, sortOptions).ToList();
            List<FileResponse> sortedFileResponses = sortedFiles.Select(f => f.ToFileResponse(_thumbnailService.GetThumbnail(f.FileId))).ToList();
            return (folderResponses, sortedFileResponses);
        }
    }
}
