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
        public BulkRetrievalService(IFoldersRepository foldersRepository, IConfiguration configuration, IFilesRepository filesRepository)
        {
            _foldersRepository = foldersRepository;
            _configuration = configuration;
            _filesRepository = filesRepository;
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
                    await using var fileStream = System.IO.File.OpenRead(file.FilePath);
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
                        await using var fileStream = System.IO.File.OpenRead(subFile.FilePath);
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
                foreach (var file in files)
                {
                    var entry = archive.CreateEntry(file.FileName);
                    using var entryStream = entry.Open();
                    await using var fileStream = System.IO.File.OpenRead(file.FilePath);
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
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f => f.FolderName.Contains(searchString));

            List<File> files = await _filesRepository.GetFilteredFiles(f => f.FileName.Contains(searchString));
            return GetResponse(folders, files, sortOptions);
        }

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllFavorites(SortOrderOptions sortOptions)
        {
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f => f.IsFavorite);

            List<File> files = await _filesRepository.GetFilteredFiles(f => f.IsFavorite);
            return GetResponse(folders, files, sortOptions);
        } //TODO Unit test this service

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllTrashes(SortOrderOptions sortOptions)
        {
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f => f.IsTrash);


            List<File> files = await _filesRepository.GetFilteredFiles(f => f.IsTrash);
            return GetResponse(folders, files, sortOptions);
        } //TODO Unit test this service

        public async Task<(List<FolderResponse> Folders, List<FileResponse> Files)> GetAllRecents()
        {
            // Get all folders with LastOpened not null
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f => f.Metadata != null && f.Metadata.LastOpened != null);
            
            // Get all files with LastOpened not null
            List<File> files = await _filesRepository.GetFilteredFiles(f => f.Metadata != null && f.Metadata.LastOpened != null);
            
            // Combine and sort by LastOpened
            var combinedItems = new List<object>();
            combinedItems.AddRange(folders.Cast<object>());
            combinedItems.AddRange(files.Cast<object>());
            
            // Sort all items by LastOpened in descending order and take top 30
            var recentItems = combinedItems
                .OrderByDescending(item => item switch
                {
                    Folder folder => folder.Metadata?.LastOpened,
                    File file => file.Metadata?.LastOpened,
                    _ => DateTime.MinValue
                })
                .Take(30)
                .ToList();
            
            // Split back into folders and files
            List<Folder> recentFolders = recentItems.OfType<Folder>().ToList();
            List<File> recentFiles = recentItems.OfType<File>().ToList();
            
            // Use existing GetResponse method with LASTOPENED_DESCENDING
            return GetResponse(recentFolders, recentFiles, SortOrderOptions.LASTOPENED_DESCENDING);
        }

        private (List<FolderResponse> Folders, List<FileResponse> Files) GetResponse(List<Folder> folders, List<File> files, SortOrderOptions sortOptions)
        {
            List<Folder> sortedFolders = Utilities.Sort(folders, sortOptions);
            List<FolderResponse> folderResponses = sortedFolders.Select(f => f.ToFolderResponse()).ToList();

            List<File> sortedFiles = Utilities.Sort(files, sortOptions).ToList();
            List<FileResponse> sortedFileResponses = sortedFiles.Select(f => f.ToFileResponse()).ToList();
            return (folderResponses, sortedFileResponses);
        }
    }
}
