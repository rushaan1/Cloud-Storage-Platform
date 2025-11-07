using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.IdentityEntites;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Services;
using CloudStoragePlatform.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Infrastructure.Repositories
{
    public class FoldersRepository : IFoldersRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserIdentification _uIdentification;
        private ApplicationUser? _user => _uIdentification.User;
        public FoldersRepository(UserIdentification uIdentification, ApplicationDbContext db) 
        {
            _db = db;
            _uIdentification = uIdentification;
        }
        public async Task<Folder> AddFolder(Folder folder) 
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            folder.UserId = _user.Id;
            folder.User = _user;
            if (folder.Metadata != null) 
            {
                folder.Metadata.User = _user;
                folder.Metadata.UserId = _user.Id;
            }
            if (folder.Sharing != null)
            {
                folder.Sharing.User = _user;
                folder.Sharing.UserId = _user.Id;
            }

            _db.Folders.Add(folder);
            await _db.SaveChangesAsync();
            return folder;
        }

        public async Task<List<Folder>> GetAllFolders() 
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            return await _db.Folders.Where(f => f.UserId == _user.Id).ToListAsync();
        }

        public async Task<List<Folder>> GetFilteredFolders(Expression<Func<Folder, bool>> predicate) 
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            return await _db.Folders.Where(f => f.UserId == _user.Id).Where(predicate).ToListAsync();
        }

        public async Task<List<Folder>> GetFilteredSubFolders(Folder parent, Func<Folder, bool> predicate)
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            // Only subfolders belonging to the user
            return await Task.Run(() => parent.SubFolders.Where(f => f.UserId == _user.Id).Where(predicate).ToList());
        }

        public async Task<Folder?> GetFolderByFolderId(Guid id) 
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            return await _db.Folders.FirstOrDefaultAsync(f => f.FolderId == id && f.UserId == _user.Id);
        }

        public async Task<Folder?> GetFolderByFolderPath(string path)
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            return await _db.Folders.FirstOrDefaultAsync(f => f.FolderPath == path && f.UserId == _user.Id);
        }

        public async Task<Folder?> UpdateFolder(Folder folder, bool updateProperties, bool updateParentFolder, bool updateMetadata, bool updateSharing, bool updateSubFolders, bool updateSubFiles) 
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            Folder? matchingFolder = await _db.Folders.FirstOrDefaultAsync(f => f.FolderId == folder.FolderId && f.UserId == _user.Id);
            if (matchingFolder== null) 
            {
                return null;
            }
            if (updateProperties) 
            {
                _db.Entry(matchingFolder).CurrentValues.SetValues(folder);
            }

            if (updateParentFolder) 
            {
                if (matchingFolder.ParentFolder != null)
                {
                    if (!matchingFolder.ParentFolder.Equals(folder.ParentFolder))
                    {
                        matchingFolder.ParentFolder = folder.ParentFolder;
                    }
                }
            }
            if (updateMetadata) 
            {
                if (!matchingFolder.Metadata.Equals(folder.Metadata))
                {
                    matchingFolder.Metadata = folder.Metadata;
                }
            }
            if (updateSharing) 
            {
                if (!matchingFolder.Sharing.Equals(folder.Sharing))
                {
                    matchingFolder.Sharing = folder.Sharing;
                }
            }

            if (updateSubFolders) 
            {
                if (!matchingFolder.SubFolders.SequenceEqual(folder.SubFolders))
                {
                    List<Folder> toRemove = matchingFolder.SubFolders.Where(existing => !folder.SubFolders.Contains(existing)).ToList();
                    List<Folder> toAdd = folder.SubFolders.Where(newItem => !matchingFolder.SubFolders.Contains(newItem)).ToList();
                    foreach (var item in toRemove)
                    {
                        matchingFolder.SubFolders.Remove(item);
                    }
                    foreach (var item in toAdd)
                    {
                        matchingFolder.SubFolders.Add(item);
                    }
                }
            }
            if (updateSubFiles)
            {
                if (!matchingFolder.Files.SequenceEqual(folder.Files))
                {
                    List<Core.Domain.Entities.File> filesToRemove = matchingFolder.Files.Where(existing => !folder.Files.Contains(existing)).ToList();
                    List<Core.Domain.Entities.File> filesToAdd = folder.Files.Where(newItem => !matchingFolder.Files.Contains(newItem)).ToList();
                    foreach (var item in filesToRemove)
                    {
                        matchingFolder.Files.Remove(item);
                    }
                    foreach (var item in filesToAdd)
                    {
                        matchingFolder.Files.Add(item);
                    }
                }
            }
            await _db.SaveChangesAsync();
            return matchingFolder;
        }

        public async Task<bool> DeleteFolder(Folder folder) 
        {
            if (_user == null) throw new InvalidOperationException("User context is not set.");
            // Only allow delete if folder belongs to user
            if (folder.UserId != _user.Id) return false;

            if (folder.SubFolders.Any()) 
            {
                foreach (Folder f in folder.SubFolders.ToList()) 
                {
                    await DeleteFolder(f);
                }
            }

            if (folder.Files.Any()) 
            {
                foreach (Core.Domain.Entities.File file in folder.Files.ToList()) 
                {
                    Utility.DisconnectAndDeleteMetadataAndSharing(_db, file);
                    System.IO.File.Delete(Path.Combine(_uIdentification.PhysicalStoragePath, file.FileId.ToString()));
                    _db.Files.Remove(file);
                }
            }

            folder.ParentFolder = null;
            folder.ParentFolderId = null;

            Utility.DisconnectAndDeleteMetadataAndSharing(_db, folder);
            _db.Folders.Remove(folder);

            // TODO Handle null cases now that metadata and sharing can be null
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
