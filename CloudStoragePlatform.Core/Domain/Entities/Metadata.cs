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
        public File? File { get; set; }
        public Folder? Folder { get; set; }
        public DateTime? PreviousReplacementDate { get; set; }
        public int? ReplaceCount { get; set; }

        public DateTime? PreviousRenameDate { get; set; }
        public int? RenameCount { get; set; }

        public string? PreviousPath { get; set; }
        public DateTime? PreviousMoveDate { get; set; }
        public int? MoveCount { get; set; }

        public DateTime? LastOpened { get; set; }
        public int? OpenCount { get; set; }

        public int? ShareCount { get; set; }
        /// <summary>
        /// in MegaBytes
        /// </summary>
        public long? Size { get; set; }

        public MetadataResponse ToMetadataResponse() 
        {
            return new MetadataResponse()
            {
                MetadataId = MetadataId,
                PreviousReplacementDate = PreviousReplacementDate,
                PreviousRenameDate = PreviousRenameDate,
                RenameCount = RenameCount,
                PreviousMoveDate = PreviousMoveDate,
                LastOpened = LastOpened,
                Size = Size,
                PreviousPath = PreviousPath,
                ShareCount = ShareCount,
                MoveCount = MoveCount,
                OpenCount = OpenCount,
                ReplaceCount = ReplaceCount
            };
        }
    }
}
