using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Exceptions
{
    public class DuplicateFileException : ArgumentException
    {
        public DuplicateFileException() : base()
        {

        }
    }
}
