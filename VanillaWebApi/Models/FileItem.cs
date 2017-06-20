using System;

namespace VanillaWebApi.Models
{
    public class FileItem : BaseObject
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public long length { get; set; }
        public bool isFolder { get; set; }
        public DateTime date { get; set; }
        public bool isReadOnly { get; set; }
    }
}