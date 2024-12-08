using CloudStoragePlatform.Core.DTO;
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
        
        public Guid? ParentFolderId { get; set; }
        public virtual Folder? ParentFolder { get; set; }
        public virtual ICollection<Folder> SubFolders { get; set; } = new List<Folder>();
        public virtual ICollection<File> Files { get; set; } = new List<File>();

        public FolderResponse ToFolderResponse() 
        {
            return new FolderResponse()
            {
                FolderId = FolderId,
                FolderName = FolderName,
                FolderPath = FolderPath,
                IsFavorite = IsFavorite,
                IsTrash = IsTrash
            };
        }
    }
}
