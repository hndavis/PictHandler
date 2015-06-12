using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFunctions
{
    public class FileInfoItem
    {
        public String FullPathName { get; set; }
        public String Type { get; set; }

        public FileInfoItem(string pathName)
        {
            FullPathName = pathName;
        }
    }

    public class FileInfoItems : List<FileInfoItem>
    {
        List<FileInfoItem> fileInfoItems = new List<FileInfoItem>();

        

        FileInfoItems(List<String> filePaths)
        {
            this.AddRange(filePaths.Select(x => new FileInfoItem(x.ToString())));
           
        }


    }
}
