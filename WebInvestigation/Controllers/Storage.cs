using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Rest;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using TonoAspNetCore;
using WebInvestigation.Models;

namespace WebInvestigation.Controllers
{
    [RequireHttps]
    public class Storage : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Index(new StorageModel
            {
                Skip = true,
                StrageAccountName = StorageModel.Default.StrageAccountName,
                Key = StorageModel.Default.Key,
                BlobContainerName = StorageModel.Default.BlobContainerName,
                BlobName = StorageModel.Default.BlobName,
                FileShareName = StorageModel.Default.FileShareName,
                FileName = StorageModel.Default.FileName,
            });
        }

        [HttpPost]
        public IActionResult Index(StorageModel model)
        {
            // Apply input history from cookie
            var cu = ControllerUtils.From(this);
            cu.PersistInput("StrageAccountName", model, StorageModel.Default.StrageAccountName);
            cu.PersistInput("Key", model, StorageModel.Default.Key);
            cu.PersistInput("Page", model, StorageModel.Default.Page);
            if (!model.Pages.Contains(model.Page))
            {
                model.Page = "Blob";
            }
            switch (model.Page)
            {
                case "Blob": return Blob(model);
                case "File": return File(model);
                case "Table": return Table(model);
                case "Queue": return Queue(model);
                default: return NotFound($"Page {model.Page} not found.");
            }
        }

        private IActionResult Blob(StorageModel model)
        {
            var cu = ControllerUtils.From(this);
            cu.PersistInput("BlobContainerName", model, StorageModel.Default.BlobContainerName);
            cu.PersistInput("BlobName", model, StorageModel.Default.BlobName);

            try
            {
                if (!model.Skip)
                {
                    var credential = new StorageSharedKeyCredential(model.StrageAccountName, model.Key);
                    var serviceUri = new Uri($"https://vnetpocstorage.{model.Page.ToLower()}.core.windows.net/");
                    var service = new BlobServiceClient(serviceUri, credential);
                    var container = service.GetBlobContainerClient(model.BlobContainerName);
                    var blob = container.GetBlobClient(model.BlobName);
                    var data = blob.Download();
                    var sr = new StreamReader(data.Value.Content);
                    model.Result = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Blob Error : {ex.Message}";
            }

            model.Skip = false;
            return View(model);
        }

        public IActionResult File(StorageModel model)
        {
            var cu = ControllerUtils.From(this);
            cu.PersistInput("FileShareName", model, StorageModel.Default.FileShareName);
            cu.PersistInput("FileName", model, StorageModel.Default.FileName);

            try
            {
                if (!model.Skip)
                {
                    var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={model.StrageAccountName};AccountKey={model.Key}");
                    var fc = storageAccount.CreateCloudFileClient();
                    var fs = fc.GetShareReference(model.FileShareName);
                    var clouddir = fs.GetRootDirectoryReference();
                    var dirs = model.FileName.Split('\\', StringSplitOptions.RemoveEmptyEntries);
                    for( var i = 0; i < dirs.Length - 1; i++)
                    {
                        var dir = dirs[i];
                        clouddir = clouddir.GetDirectoryReference(dir);
                    }
                    var fr = clouddir.GetFileReference(dirs[dirs.Length - 1]);
                    model.Result = fr.DownloadTextAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"File Share Error : {ex.Message}";
            }

            model.Skip = false;
            return View(model);
        }
        public IActionResult Table(StorageModel model)
        {
            return View(model);
        }
        public IActionResult Queue(StorageModel model)
        {
            return View(model);
        }
    }
}
