using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.DTO
{
    public class RenameRequest
    {
        [Required]
        public Guid id { get; set; }
        [Required]
        public string newName { get; set; }
    }
}
