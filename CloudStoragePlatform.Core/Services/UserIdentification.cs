using CloudStoragePlatform.Core.Domain.IdentityEntites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Services
{
    public class UserIdentification
    {
        public ApplicationUser? User { get; set; }
        public string PhysicalStoragePath => "C:\\CloudStoragePlatform\\" + User.Id.ToString();
        public UserIdentification() { Console.WriteLine("Instantiated UserIdentification "+ToString()); }
    }
}
