using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using File = CloudStoragePlatform.Core.Domain.Entities.File;

namespace CloudStoragePlatform.Core.Services
{
    public class FoldersRetrievalService : IFoldersRetrievalService
    {
        private readonly IFoldersRepository _foldersRepository;
        private readonly IFilesRepository _fileRepository;
        public FoldersRetrievalService(IFoldersRepository foldersRepository, IConfiguration configuration, IFilesRepository fileRepository)
        {
            _foldersRepository = foldersRepository;
            _fileRepository = fileRepository;
        }
        public async Task<FolderResponse?> GetFolderByFolderId(Guid fid)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(fid);
            if (folder == null)
            {
                return null;
            }
            return folder.ToFolderResponse();
        }

        public async Task<MemoryStream> DownloadFolder(Guid fid) 
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(fid);
            if (folder == null)
            {
                return null;
            }
            List<File> allSubFilesUptoMaxDepth = await _fileRepository.GetFilteredFiles(f => f.FilePath.Contains(folder.FolderPath));
            using MemoryStream zipStream = new MemoryStream();
            using ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true);
            foreach (File subFile in allSubFilesUptoMaxDepth)
            {
                string path = $"{folder.FolderName}/"+subFile.FilePath.Replace(folder.FolderPath, "").Replace("\\", "/");
                var entry = archive.CreateEntry(path);
                using var entryStream = entry.Open();
                await using var fileStream = System.IO.File.OpenRead(subFile.FilePath);
                await fileStream.CopyToAsync(entryStream);
            }
            zipStream.Seek(0, SeekOrigin.Begin);
            return zipStream;
        }

        public async Task<FolderResponse?> GetFolderByFolderPath(string fpath)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderPath(fpath);
            if (folder == null)
            {
                return null;
            }
            return folder.ToFolderResponse();
        }

        public async Task<FileOrFolderMetadataResponse> GetMetadata(Guid fid)
        {
            Folder? folder = await _foldersRepository.GetFolderByFolderId(fid);
            if (folder == null)
            {
                throw new ArgumentException();
            }
            FileOrFolderMetadataResponse response = folder!.Metadata!.ToMetadataResponse();
            response.CreationDate = folder.CreationDate;
            response.SubFilesCount = folder.Files.Count;
            response.SubFoldersCount = folder.SubFolders.Count;
            response.ParentFolderName = folder.ParentFolder!.FolderName;
            return response;
        }
    }
}
