using CloudStoragePlatform.Core.Domain.IdentityEntites;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.Entities
{
    public class UserSession
    {
        [Key]
        public Guid UserSessionId { get; set; } = Guid.NewGuid();

        [Required]
        public string RefreshToken { get; set; } = null!;

        [Required]
        public DateTime RefreshTokenExpirationDateTime { get; set; }

        [Required]
        public Guid ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
