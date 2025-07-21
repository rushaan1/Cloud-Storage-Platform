using CloudStoragePlatform.Core.Domain.IdentityEntites;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.Entities
{
    public class Sharing
    {
        [Key]
        public Guid SharingId { get; set; }
        public virtual File? File { get; set; }
        public virtual Folder? Folder { get; set; }
        public string? ShareLinkUrl { get; set; }
        public DateTime? ShareLinkExpiry { get; set; } 
        public int? CurrentShareLinkTimesSeen { get; set; } 
        public DateTime? ShareLinkCreateDate { get; set; }
        public virtual ApplicationUser User { get; set; }
        public Guid UserId { get; set; }
    }
}
