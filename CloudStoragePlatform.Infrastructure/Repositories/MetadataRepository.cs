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
    public class MetadataRepository
    {
        private readonly ApplicationDbContext _db;
        public MetadataRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Metadata?> GetMetadataByMetadatId(Guid id)
        {
            return await _db.MetaDatasets.FirstOrDefaultAsync(f => f.MetadataId == id);
        }

        public async Task<Metadata?> UpdateSharing(Metadata metadata)
        {
            Metadata? matchingMetadata = await _db.MetaDatasets.FirstOrDefaultAsync(m => m.MetadataId == metadata.MetadataId);
            if (matchingMetadata == null)
            {
                return null;
            }
            _db.Entry(matchingMetadata).CurrentValues.SetValues(metadata);
            await _db.SaveChangesAsync();
            return matchingMetadata;
        }
    }
}
