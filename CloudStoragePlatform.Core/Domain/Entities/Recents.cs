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
        public virtual ICollection<Folder> RecentFolders { get; set; } = new List<Folder>();
        public virtual ICollection<File> RecentFiles { get; set; } = new List<File>();
    }
}
