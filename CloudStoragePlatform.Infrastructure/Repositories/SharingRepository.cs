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

        public async Task<Sharing> CreateSharing(Sharing sharing)
        {
            _db.Shares.Add(sharing);
            await _db.SaveChangesAsync();
            return sharing;
        }

        public async Task<Sharing?> GetSharingById(Guid id)
        {
            return await _db.Shares.FirstOrDefaultAsync(s => s.SharingId == id);
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

        public async Task<bool> DeleteSharing(Sharing sharing)
        {
            _db.Shares.Remove(sharing);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<int> IncrementVisits(Guid sharingId)
        {
            var sharing = await _db.Shares.FirstOrDefaultAsync(s => s.SharingId == sharingId);
            if (sharing == null)
            {
                return 0;
            }
            sharing.Visits = (sharing.Visits ?? 0) + 1;
            await _db.SaveChangesAsync();
            return sharing.Visits.Value;
        }

        public async Task SetVisits(Guid sharingId, int visits)
        {
            var sharing = await _db.Shares.FirstOrDefaultAsync(s => s.SharingId == sharingId);
            if (sharing == null)
            {
                return;
            }
            sharing.Visits = visits;
            await _db.SaveChangesAsync();
        }
    }
}
