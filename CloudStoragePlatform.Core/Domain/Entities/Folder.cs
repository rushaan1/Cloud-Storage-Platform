using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.Entities
{
    public class Folder : BaseForFileFolder
    {
        [Key]
        public Guid FolderId { get; set; }
        
        public string FolderName { get; set; }
        
        public string FolderPath { get; set; }
        
        public Guid? FolderParentId { get; set; }
        public Folder? ParentFolder { get; set; }
        public long? FolderSize { get; set; }
        public ICollection<Folder>? SubFolders { get; set; }
        public ICollection<File>? Files { get; set; }
    }
}
