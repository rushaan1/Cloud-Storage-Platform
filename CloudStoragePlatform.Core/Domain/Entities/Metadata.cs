using CloudStoragePlatform.Core.Domain.IdentityEntites;
using CloudStoragePlatform.Core.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.Entities
{
    public class Metadata
    {
        [Key]
        public Guid MetadataId { get; set; }
        public virtual File? File { get; set; }
        public virtual Folder? Folder { get; set; }

        public DateTime? PreviousRenameDate { get; set; }
        public int RenameCount { get; set; } = 0;

        public string? PreviousPath { get; set; }
        public DateTime? PreviousMoveDate { get; set; }
        public int MoveCount { get; set; } = 0;

        public DateTime? LastOpened { get; set; }
        public int OpenCount { get; set; } = 0;

        public int ShareCount { get; set; } = 0;
        public virtual ApplicationUser User { get; set; }
        public Guid UserId { get; set; }

        /// <summary>
        /// in MegaBytes
        /// </summary>

        public FileOrFolderMetadataResponse ToMetadataResponse() 
        {
            var response = new FileOrFolderMetadataResponse()
            {
                MetadataId = MetadataId,
                PreviousRenameDate = PreviousRenameDate,
                RenameCount = RenameCount,
                PreviousMoveDate = PreviousMoveDate,
                LastOpened = LastOpened,
                PreviousPath = PreviousPath,
                ShareCount = ShareCount,
                MoveCount = MoveCount,
                OpenCount = OpenCount,
            };
            return response;
        }
    }
}
