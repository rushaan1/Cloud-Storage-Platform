using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.DTO
{
    public class FolderRenameRequest
    {
        [Required]
        public Guid FolderId { get; set; }
        [Required]
        public string FolderNewName { get; set; }
    }
}
