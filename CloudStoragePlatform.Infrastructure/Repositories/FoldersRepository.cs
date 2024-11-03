using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Infrastructure.Repositories
{
    public class FoldersRepository
    {
        private readonly ApplicationDbContext _db;
        public FoldersRepository(ApplicationDbContext db) 
        {
            _db = db;
        }
        public async Task<Folder> AddFolder(Folder folder) 
        {
            _db.Folders.Add(folder);
            await _db.SaveChangesAsync();
            return folder;
        }

        public async Task<List<Folder>> GetAllFolders(bool includeNavigationProperties) 
        {
            var folders = await _db.Folders.ToListAsync();
            await LoadingHelperMethods.LoadFolderNavigationPropertiesForMultipleEntities(_db, folders, includeNavigationProperties);
            return folders;
        }

        public async Task<List<Folder>> GetAllSubFolders(Folder folder, bool includeNavigationProperties)
        {
            await _db.Entry(folder!).Collection(f => f.SubFolders).LoadAsync();
            var folders = folder!.SubFolders?.ToList();
            if (folders == null) 
            {
                return new List<Folder>();
            }
            await LoadingHelperMethods.LoadFolderNavigationPropertiesForMultipleEntities(_db, folders, includeNavigationProperties);
            return folders;
        }

        public async Task<List<Core.Domain.Entities.File>> GetAllSubFiles(Folder folder, bool includeNavigationProperties)
        {
            await _db.Entry(folder!).Collection(f => f.Files).LoadAsync();
            var files = folder!.Files?.ToList();
            if (files == null) 
            {
                return new List<Core.Domain.Entities.File>();
            }
            await LoadingHelperMethods.LoadFileNavigationPropertiesForMultipleEntities(_db, files, includeNavigationProperties);
            return files;
        }

        public async Task<Folder?> GetFolderByFolderId(Guid id, bool includeNavigationProperties) 
        {
            Folder? folder = await _db.Folders.FirstOrDefaultAsync(f => f.FolderId == id);
            if (folder == null) 
            {
                return null;
            }
            await LoadingHelperMethods.LoadFolderNavigationProperties(_db, folder, includeNavigationProperties);
            return folder;
        }

        public async Task<Folder?> GetFolderByFolderPath(string path, bool includeNavigationProperties)
        {
            Folder? folder = await _db.Folders.FirstOrDefaultAsync(f => f.FolderPath == path);
            if (folder == null)
            {
                return null;
            }
            await LoadingHelperMethods.LoadFolderNavigationProperties(_db, folder, includeNavigationProperties);
            return folder;
        }

        public async Task<Folder> UpdateFolder(Folder folder, bool updateProperties, bool updateParentFolder, bool updateMetadata, bool updateSharing, bool updateSubFolders, bool updateSubFiles) 
        {
            Folder? matchingFolder = await _db.Folders.FirstOrDefaultAsync(f => f.FolderId == folder.FolderId);
            if (matchingFolder== null) 
            {
                return folder;
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

        public async Task<bool> DeleteFolderById(Guid folderId) 
        {
            Folder? folder = await _db.Folders.FirstOrDefaultAsync(f => f.FolderId == folderId);
            if (folder == null) 
            {
                return false;
            }

            await LoadingHelperMethods.LoadFolderNavigationProperties(_db, folder, true);
            
            _db.Shares.Remove(folder.Sharing);
            _db.MetaDatasets.Remove(folder.Metadata);

            if (folder.SubFolders.Any()) 
            {
                _db.Folders.RemoveRange(folder.SubFolders);
            }
            if (folder.Files.Any()) 
            {
                _db.Files.RemoveRange(folder.Files);
            }

            _db.Folders.Remove(folder);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
