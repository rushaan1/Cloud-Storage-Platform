using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.DTO
{
    public class FolderAddRequest
    {
        [Required]
        public string FolderName { get; set; }
        /// <summary>
        /// PATH MUST INCLUDE NEW FOLDER'S NAME!
        /// </summary>
        [Required]
        public string FolderPath { get; set; }
    }
}
