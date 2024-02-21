using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService
{
    public abstract class FileModelBase
    {
        public string FilePath { get; set; }
        public string KeyName { get; set; }
    }

    public class S3FileModel : FileModelBase
    {
        public string BucketName { get; set; }
    }

    public class AzureBlobFileModel : FileModelBase
    {
        public string ContainerName { get; set; }
    }

}
