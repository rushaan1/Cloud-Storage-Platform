using Cloud_Storage_Platform.CustomValidationAttributes;
using CloudStoragePlatform.Core.CustomValidationAttributes;
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
        [FileOrFolderNameValidationAttribute("Invalid folder name")]
        public string FolderName { get; set; }
        /// <summary>
        /// PATH MUST INCLUDE NEW FOLDER'S NAME!
        /// </summary>
        [Required]
        [PathEndingMatchingWithString(nameof(FolderName))]
        public string FolderPath { get; set; }
    }
}
