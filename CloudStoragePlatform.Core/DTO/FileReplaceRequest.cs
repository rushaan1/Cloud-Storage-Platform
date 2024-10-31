using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.DTO
{
    public class FileReplaceRequest
    {
        [Required]
        public Guid FileId { get; set; }
        [Required]
        public FileAddRequest NewFile { get; set; }
    }
}
