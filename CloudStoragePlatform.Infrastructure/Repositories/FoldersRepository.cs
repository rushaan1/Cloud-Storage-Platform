using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
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
        public FoldersRepository(ApplicationDbContext db) 
        {
            _db = db;

            // Not remembering sql queries no problem

            // Deleting all folders with metadatsets & sharing except home
            //List<Folder> folders = _db.Folders.Where(f=>f.FolderName!= "home").ToList();
            //folders.ForEach(f => DisconnectAndDeleteMetadataAndSharing(f));
            //folders.ForEach(f => _db.Folders.Remove(f));
            //_db.SaveChanges();
        }
        public async Task<Folder> AddFolder(Folder folder) 
        {
            _db.Folders.Add(folder);
            await _db.SaveChangesAsync();
            return folder;
        }

        public async Task<List<Folder>> GetAllFolders() 
        {
            return await _db.Folders.ToListAsync();
        }

        public async Task<List<Folder>> GetFilteredFolders(Expression<Func<Folder, bool>> predicate) 
        {
            return await _db.Folders.Where(predicate).ToListAsync();
        }

        public async Task<List<Folder>> GetFilteredSubFolders(Folder parent, Func<Folder, bool> predicate)
        {
            return await Task.Run(()=>parent.SubFolders.Where(predicate).ToList());
        }

        public async Task<List<Core.Domain.Entities.File>> GetFilteredSubFiles(Folder parent, Func<Core.Domain.Entities.File, bool> predicate)
        {
            return await Task.Run(()=>parent.Files.Where(predicate).ToList());
        }

        public async Task<Folder?> GetFolderByFolderId(Guid id) 
        {
            return await _db.Folders.FirstOrDefaultAsync(f => f.FolderId == id);
        }

        public async Task<Folder?> GetFolderByFolderPath(string path)
        {
            return await _db.Folders.FirstOrDefaultAsync(f => f.FolderPath == path);
        }

        public async Task<Folder?> UpdateFolder(Folder folder, bool updateProperties, bool updateParentFolder, bool updateMetadata, bool updateSharing, bool updateSubFolders, bool updateSubFiles) 
        {
            Folder? matchingFolder = await _db.Folders.FirstOrDefaultAsync(f => f.FolderId == folder.FolderId);
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
                    DisconnectAndDeleteMetadataAndSharing(file);
                    _db.Files.Remove(file);
                }
            }

            folder.ParentFolder = null;
            folder.ParentFolderId = null;

            DisconnectAndDeleteMetadataAndSharing(folder);
            _db.Folders.Remove(folder);

            // TODO Handle null cases now that metadata and sharing can be null
            return await _db.SaveChangesAsync() > 0;
        }

        // Utility Function
        private void DisconnectAndDeleteMetadataAndSharing(BaseForFileFolder entity) 
        {
            if (entity.Metadata != null)
            {
                Metadata metadata = entity.Metadata;
                metadata.File = null;
                metadata.Folder = null;

                entity.Metadata = null;
                entity.MetadataId = null;
                _db.MetaDatasets.Remove(metadata);
            }
            if (entity.Sharing != null)
            {
                Sharing sharing = entity.Sharing;
                sharing.File = null;
                sharing.Folder = null;

                entity.Sharing = null;
                entity.SharingId = null;
                _db.Shares.Remove(sharing);
            }
        }
    }
}
