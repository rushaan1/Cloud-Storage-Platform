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
            List<Folder> folders = await _foldersRepository.GetFilteredFolders(f => f.FolderName.ToLower().Contains(searchString.ToLower()));

            List<File> files = await _filesRepository.GetFilteredFiles(f => f.FileName.ToLower().Contains(searchString.ToLower()));
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
