using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Infrastructure.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Infrastructure
{
    public static class Utility
    {
        public static void DisconnectAndDeleteMetadataAndSharing(ApplicationDbContext _db, BaseForFileFolder entity)
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
