using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.DTO
{
    public class BulkResponse
    {
        public List<FolderResponse> folders { get; set; }
        public List<FileResponse> files { get; set; }
    }
}
