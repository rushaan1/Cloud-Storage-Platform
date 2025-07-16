using CloudStoragePlatform.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.IdentityEntites
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? PersonName { get; set; }
        public string? Country { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();

    }
}
