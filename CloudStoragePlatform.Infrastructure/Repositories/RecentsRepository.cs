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
    public class RecentsRepository
    {
        private readonly ApplicationDbContext _db;
        public RecentsRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Recents?> GetRecentsById(Guid id) 
        {
            return await _db.Recents.FirstOrDefaultAsync(r => r.RecentId == id);
        }

        public async Task<Recents?> UpdateRecents(Recents recents, bool updateFolders, bool updateFiles) 
        {
            Recents? matchingRecents = await _db.Recents.FirstOrDefaultAsync(r=>r.RecentId==recents.RecentId);
            if (matchingRecents == null) 
            {
                return null;
            }
            if (updateFolders) 
            {
                List<Folder> toRemove = matchingRecents.RecentFolders.Where(rfo => !recents.RecentFolders.Contains(rfo)).ToList();
                List<Folder> toAdd = recents.RecentFolders.Where(rfo => !matchingRecents.RecentFolders.Contains(rfo)).ToList();
                foreach (var item in toRemove)
                {
                    matchingRecents.RecentFolders.Remove(item);
                }
                foreach (var item in toAdd)
                {
                    matchingRecents.RecentFolders.Add(item);
                }
            }
            if (updateFiles) 
            {
                List<Core.Domain.Entities.File> toRemove = matchingRecents.RecentFiles.Where(rfi => !recents.RecentFiles.Contains(rfi)).ToList();
                List<Core.Domain.Entities.File> toAdd = recents.RecentFiles.Where(rfi => !matchingRecents.RecentFiles.Contains(rfi)).ToList();
                foreach (var item in toRemove)
                {
                    matchingRecents.RecentFiles.Remove(item);
                }
                foreach (var item in toAdd)
                {
                    matchingRecents.RecentFiles.Add(item);
                }
            }

            await _db.SaveChangesAsync();
            return matchingRecents;
        }
    }
}
