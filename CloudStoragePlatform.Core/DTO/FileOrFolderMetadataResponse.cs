using CloudStoragePlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.DTO
{
    public class FileOrFolderMetadataResponse
    {
        public string ParentFolderName { get; set; }
        public int SubFoldersCount { get; set; }
        public int SubFilesCount { get; set; }
        public DateTime CreationDate { get; set; }
        public Guid MetadataId { get; set; }

        public DateTime? PreviousRenameDate { get; set; }
        public int RenameCount { get; set; }

        public string? PreviousPath { get; set; }
        public DateTime? PreviousMoveDate { get; set; }
        public int MoveCount { get; set; }

        public DateTime? LastOpened { get; set; }
        public int OpenCount { get; set; }

        public int ShareCount { get; set; }
        public float Size { get; set; }
    }
}
