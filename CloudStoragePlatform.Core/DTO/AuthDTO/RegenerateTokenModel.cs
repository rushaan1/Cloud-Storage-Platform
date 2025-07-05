using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.DTO.AuthDTO
{
    public class RegenerateTokenModel
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
