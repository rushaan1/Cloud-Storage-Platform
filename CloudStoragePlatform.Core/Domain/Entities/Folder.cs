using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.Entities
{
    public class Folder
    {
        public Guid FolderId { get; set; }
        [Required]
        public string FolderName { get; set; }
        [Required]
        public string FolderPath { get; set; }
        public Guid? FolderParentId { get; set; }
        public Guid? FolderMetadataId { get; set; }
        [Required]
        public bool IsTrash { get; set; }
        [Required]
        public bool IsFavorite { get; set; }
        [Required]
        public long FolderSize { get; set; }
        public Guid? SharingId { get; set; }
        [Required]
        public Sharing Sharing { get; set; }
    }
}
