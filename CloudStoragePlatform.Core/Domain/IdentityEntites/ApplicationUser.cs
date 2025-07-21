using CloudStoragePlatform.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = CloudStoragePlatform.Core.Domain.Entities.File;

namespace CloudStoragePlatform.Core.Domain.IdentityEntites
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? PersonName { get; set; }
        public string? Country { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
        public virtual ICollection<Folder> Folders { get; set; }
        public virtual ICollection<File> Files { get; set; }
        public virtual ICollection<Sharing> Shares { get; set; }
        public virtual ICollection<Metadata> MetaDatasets { get; set; }
    }
}
