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
    public class FilesRepository
    {
        private readonly ApplicationDbContext _db;
        public FilesRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Core.Domain.Entities.File> AddFile(Core.Domain.Entities.File file)
        {
            _db.Files.Add(file);
            await _db.SaveChangesAsync();
            return file;
        }

        public async Task<List<Core.Domain.Entities.File>> GetAllFiles()
        {
            return await _db.Files.ToListAsync();
        }

        public async Task<Core.Domain.Entities.File?> GetFileByFileId(Guid id)
        {
            return await _db.Files.FirstOrDefaultAsync(f => f.FileId == id);
        }

        public async Task<Core.Domain.Entities.File?> GetFileByFilePath(string path)
        {
            return await _db.Files.FirstOrDefaultAsync(f => f.FilePath == path);
        }


        public async Task<Core.Domain.Entities.File?> UpdateFolder(Core.Domain.Entities.File file, bool updateProperties, bool updateParentFolder, bool updateMetadata, bool updateSharing)
        {
            Core.Domain.Entities.File? matchingFile = await _db.Files.FirstOrDefaultAsync(f => f.FileId == file.FileId);
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
            _db.Shares.Remove(file.Sharing);
            _db.MetaDatasets.Remove(file.Metadata);

            _db.Files.Remove(file);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
