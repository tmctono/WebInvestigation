using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebInvestigation.Controllers;

namespace WebInvestigation.Models
{
    public class StorageModel
    {
        public static readonly StorageModel Default = new StorageModel
        {
            Page = "(DUMMY)",
            StrageAccountName = "(Your Storage Account Name)",
            Key = "(Your Storaget Key1 or Key2)",
            BlobContainerName = "(Your Blob Container Name)",
            BlobName = "(Your Blob Name)",
            FileShareName = "(Your File Share Name)",
            FileName = "\\(your dir name)\\(your file name)",
        };
        public string Page { get; set; }
        public string StrageAccountName { get; set; }
        public string Key { get; set; }
        public string[] Pages { get; set; } = new string[] { "Blob", "File", "Table", "Queue" };
        public bool Skip { get; set; }
        public string ErrorMessage { get; set; }
        public string Result { get; set; }

        //=== BLOB ===
        public string BlobContainerName { get; set; }
        public string BlobName { get; set; }

        //=== FILE ===
        public string FileShareName { get; set; }
        public string FileName { get; set; }    // \dir1\todo.txt

    }
}
