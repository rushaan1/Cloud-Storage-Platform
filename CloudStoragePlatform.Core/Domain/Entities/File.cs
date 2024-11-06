using CloudStoragePlatform.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.Entities
{
    public class File : BaseForFileFolder
    {
        [Key]
        public Guid FileId { get; set; }
        
        public string FileName { get; set; }
        
        public string FilePath { get; set; }
        
        public long FileSize { get; set; }
        
        public FileTypes FileType { get; set; }
        
        public Guid ParentFolderId { get; set; }
        public virtual Folder ParentFolder { get; set; }
    }
}
