using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.Entities
{
    public class BaseForFileFolder
    {
        
        public Guid SharingId { get; set; }
        
        public Sharing Sharing { get; set; }
        
        public Guid MetadataId { get; set; }
        
        public Metadata Metadata { get; set; }
        
        public DateTime CreationDate { get; set; }
        
        public bool IsFavorite { get; set; }
        
        public bool IsTrash { get; set; }
    }
}
