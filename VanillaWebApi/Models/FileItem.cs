using System;
using System.Collections.Generic;

namespace VanillaWebApi.Models
{
    public class DirectoryItem
    {
        public DirectoryItem()
        {
            fileItems = new List<FileItem>();
        }

        public ActionItem uploadAction { get; set; }
        public List<FileItem> fileItems { get; set; }
    }

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