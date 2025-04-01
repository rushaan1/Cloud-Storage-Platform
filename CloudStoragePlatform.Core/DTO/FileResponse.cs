using CloudStoragePlatform.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.DTO
{
    public class FileResponse
    {
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public bool? IsFavorite { get; set; }
        public bool? IsTrash { get; set; }
        public FileType FileType { get; set; }
    }
}
