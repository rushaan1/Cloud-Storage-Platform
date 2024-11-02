using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.Entities
{
    public class Recents
    {
        [Key]
        public Guid RecentId { get; set; }
        public ICollection<Folder>? RecentFolders { get; set;}
        public ICollection<File>? RecentFiles { get; set; }
    }
}
