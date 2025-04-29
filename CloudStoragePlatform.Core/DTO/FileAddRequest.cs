using CloudStoragePlatform.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.DTO
{
    public class FileAddRequest
    {
        [Required]
        public string FileName { get; set; }
        [Required]
        public string FilePath { get; set; }
        [Required]
        public FileType FileType { get; set; }
    }
}
