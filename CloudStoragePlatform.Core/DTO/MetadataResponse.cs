using CloudStoragePlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.DTO
{
    public class MetadataResponse
    {
        public Guid MetadataId { get; set; }
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
    }
}
