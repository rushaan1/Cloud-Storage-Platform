using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.IdentityEntites;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.Services;
using CloudStoragePlatform.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Infrastructure.Repositories
{
    public class FilesRepository : IFilesRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserIdentification _uIdentification;
        private ApplicationUser? _user => _uIdentification.User;
        public FilesRepository(UserIdentification uIdentification, ApplicationDbContext db)
        {
            _db = db;
            _uIdentification = uIdentification;
        }

        public async Task<Core.Domain.Entities.File> AddFile(Core.Domain.Entities.File file)
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            file.UserId = _user.Id;
            file.User = _user;
            if (file.Metadata != null)
            {
                file.Metadata.User = _user;
                file.Metadata.UserId = _user.Id;
            }
            if (file.Sharing != null)
            {
                file.Sharing.User = _user;
                file.Sharing.UserId = _user.Id;
            }
            _db.Files.Add(file);
            await _db.SaveChangesAsync();
            return file;
        }

        public async Task<List<Core.Domain.Entities.File>> GetAllFiles()
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            return await _db.Files.Where(f => f.UserId == _user.Id).ToListAsync();
        }

        public async Task<Core.Domain.Entities.File?> GetFileByFileId(Guid id)
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            return await _db.Files.FirstOrDefaultAsync(f => f.FileId == id && f.UserId == _user.Id);
        }

        public async Task<Core.Domain.Entities.File?> GetFileByFilePath(string path)
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            return await _db.Files.FirstOrDefaultAsync(f => f.FilePath == path && f.UserId == _user.Id);
        }

        public async Task<List<Core.Domain.Entities.File>> GetFilteredFiles(Func<Core.Domain.Entities.File, bool> predicate)
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            // Only files belonging to the user, then apply predicate in memory
            return await Task.Run(() => _db.Files.Where(f => f.UserId == _user.Id).AsEnumerable().Where(predicate).ToList());
        }

        public async Task<Core.Domain.Entities.File?> UpdateFile(Core.Domain.Entities.File file, bool updateProperties, bool updateParentFolder, bool updateMetadata, bool updateSharing)
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            Core.Domain.Entities.File? matchingFile = await _db.Files.FirstOrDefaultAsync(f => f.FileId == file.FileId && f.UserId == _user.Id);
            if (matchingFile == null)
            {
                return null;
            }
            if (updateProperties)
            {
                _db.Entry(matchingFile).CurrentValues.SetValues(file);
            }

            if (updateParentFolder)
            {
                if (matchingFile.ParentFolder != null)
                {
                    if (!matchingFile.ParentFolder.Equals(file.ParentFolder))
                    {
                        matchingFile.ParentFolder = file.ParentFolder;
                    }
                }
            }
            if (updateMetadata)
            {
                if (!matchingFile.Metadata.Equals(file.Metadata))
                {
                    matchingFile.Metadata = file.Metadata;
                }
            }
            if (updateSharing)
            {
                if (!matchingFile.Sharing.Equals(file.Sharing))
                {
                    matchingFile.Sharing = file.Sharing;
                }
            }
            await _db.SaveChangesAsync();
            return matchingFile;
        }

        public async Task<bool> DeleteFile(Core.Domain.Entities.File file)
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            if (file.UserId != _user.Id) return false;

            _db.Shares.Remove(file.Sharing);
            _db.MetaDatasets.Remove(file.Metadata);

            _db.Files.Remove(file);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
