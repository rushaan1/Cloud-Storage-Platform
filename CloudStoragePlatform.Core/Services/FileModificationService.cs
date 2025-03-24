using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Services
{
    public class FileModificationService : IFilesModificationService
    {
        private readonly IFoldersRepository _foldersRepository;
        private readonly IFilesRepository _filesRepository;
        public FileModificationService(IFilesRepository filesRepository)
        {
            _foldersRepository = foldersRepository;
            _filesRepository = filesRepository;
            // inject user identifying stuff in constructor and in repository's constructor
        }

        public Task<FileResponse> AddFile(FileAddRequest fileAddRequest)
        {
            throw new NotImplementedException();
        }

        public Task<FileResponse> AddOrRemoveFavorite(Guid fileId)
        {
            throw new NotImplementedException();
        }

        public Task<FileResponse> AddOrRemoveTrash(Guid fileId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteFile(Guid fileId)
        {
            throw new NotImplementedException();
        }

        public Task<FileResponse> MoveFile(Guid fileId, string newFilePath)
        {
            throw new NotImplementedException();
        }

        public Task<FileResponse> RenameFile(FileRenameRequest fileRenameRequest)
        {
            throw new NotImplementedException();
        }
    }
}
