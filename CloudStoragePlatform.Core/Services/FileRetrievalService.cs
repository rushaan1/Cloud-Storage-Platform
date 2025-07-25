﻿using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using CloudStoragePlatform.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Services
{
    public class FileRetrievalService : IFilesRetrievalService
    {
        private readonly IFilesRepository _filesRepository;
        private readonly IConfiguration _configuration;
        private readonly UserIdentification _userIdentification;
        private readonly ThumbnailService _thumbnailService;
        public FileRetrievalService(IFilesRepository filesRepository, IConfiguration configuration, UserIdentification userIdentification, ThumbnailService thumbnailService)
        {
            _filesRepository = filesRepository;
            _configuration = configuration;
            _userIdentification = userIdentification;
            _thumbnailService = thumbnailService;
        }

        public async Task<FileStream> GetFilePreview(string filePath) 
        {
            var f = await _filesRepository.GetFileByFilePath(filePath);
            await Utilities.UpdateMetadataOpen(f, _filesRepository);
            return new FileStream(Path.Combine(_userIdentification.PhysicalStoragePath, f.FileId.ToString()), FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 64 * 1024, useAsync: true);
        }

        public async Task<FileResponse?> GetFileByFileId(Guid id)
        {
            var file = await _filesRepository.GetFileByFileId(id);
            if (file == null)
            {
                return null;
            }
            var thumbnail = _thumbnailService.GetThumbnail(file.FileId);
            return file.ToFileResponse(thumbnail);
        }

        public async Task<FileResponse?> GetFileByFilePath(string path)
        {
            var file = await _filesRepository.GetFileByFilePath(path);
            if (file == null)
            {
                return null;
            }
            var thumbnail = _thumbnailService.GetThumbnail(file.FileId);
            return file.ToFileResponse(thumbnail);
        }

        public async Task<FileOrFolderMetadataResponse> GetMetadata(Guid fileId)
        {
            var file = await _filesRepository.GetFileByFileId(fileId);
            if (file == null)
            {
                throw new ArgumentException();
            }
            var response = file!.Metadata!.ToMetadataResponse();
            response.CreationDate = file.CreationDate;
            response.ParentFolderName = file.ParentFolder.FolderName;
            response.Size = file.Size;
            return response;
        }
    }
}
