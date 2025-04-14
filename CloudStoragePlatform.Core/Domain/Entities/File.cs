using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using CloudStoragePlatform.Core.Services;
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
                
        public FileType FileType { get; set; }
        
        public Guid ParentFolderId { get; set; }
        public virtual Folder ParentFolder { get; set; }

        public FileResponse ToFileResponse()
        {
            byte[]? bytes = ThumbnailService.GetThumbnail(FileId);
            return new FileResponse()
            {
                FileId = FileId,
                FileName = FileName,
                FilePath = FilePath,
                IsFavorite = IsFavorite,
                IsTrash = IsTrash, 
                FileType = FileType,
                Thumbnail = bytes
            };
        }
    }
}
