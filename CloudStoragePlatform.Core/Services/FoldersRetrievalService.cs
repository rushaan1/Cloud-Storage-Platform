using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Services
{
    public class FoldersRetrievalService : IFoldersRetrievalService
    {
        private readonly IFoldersRepository _foldersRepository;
        public FoldersRetrievalService(IFoldersRepository foldersRepository, IConfiguration configuration)
        {
            _foldersRepository = foldersRepository;
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
