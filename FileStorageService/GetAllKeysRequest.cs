using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService
{
    public class GetAllKeysRequest
    {
        public string BucketOrContainer { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
