using CloudStoragePlatform.Core.Domain.IdentityEntites;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.Entities
{
    public class BaseForFileFolder
    {
        
        public Guid? SharingId { get; set; }
        
        public virtual Sharing? Sharing { get; set; }
        public float Size { get; set; } = 0;
        public Guid? MetadataId { get; set; }
        
        public virtual Metadata? Metadata { get; set; }
        
        public DateTime CreationDate { get; set; }

        public bool IsFavorite { get; set; } = false;

        public bool IsTrash { get; set; } = false;

        public virtual ApplicationUser User { get; set; }
        public Guid UserId { get; set; }
    }
}
