using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Infrastructure.Repositories
{
    public class SharingRepository : ISharingRepository
    {
        private readonly ApplicationDbContext _db;
        public SharingRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Sharing?> GetSharingById(Guid id)
        {
            return await _db.Shares.FirstOrDefaultAsync(s => s.SharingId == id);
        }

        public async Task<Sharing?> GetSharingByUrl(string url) 
        {
            return await _db.Shares.FirstOrDefaultAsync(s => s.ShareLinkUrl == url);
        }

        public async Task<Sharing?> UpdateSharing(Sharing sharing) 
        {
            Sharing? matchingSharing = await _db.Shares.FirstOrDefaultAsync(s=>s.SharingId == sharing.SharingId);
            if (matchingSharing == null) 
            {
                return null;
            }
            _db.Entry(matchingSharing).CurrentValues.SetValues(sharing);
            await _db.SaveChangesAsync();
            return matchingSharing;
        }
    }
}
