using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
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
                Page = StorageModel.Default.Page,
                StrageAccountName = StorageModel.Default.StrageAccountName,
                Key = StorageModel.Default.Key,
                BlobContainerName = StorageModel.Default.BlobContainerName,
                BlobName = StorageModel.Default.BlobName,
                FileShareName = StorageModel.Default.FileShareName,
                FileName = StorageModel.Default.FileName,
                TableName = StorageModel.Default.TableName,
                TablePartition = StorageModel.Default.TablePartition,
                TableKey = StorageModel.Default.TableKey,
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
                    for (var i = 0; i < dirs.Length - 1; i++)
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
            var cu = ControllerUtils.From(this);
            cu.PersistInput("TableName", model, StorageModel.Default.TableName);
            cu.PersistInput("TablePartition", model, StorageModel.Default.TablePartition);
            cu.PersistInput("TableKey", model, StorageModel.Default.TableKey);

            try
            {
                if (!model.Skip)
                {
                    var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={model.StrageAccountName};AccountKey={model.Key}");
                    var fc = storageAccount.CreateCloudTableClient();
                    var tr = fc.GetTableReference(model.TableName);
                    var to = TableOperation.Retrieve(model.TablePartition, model.TableKey);
                    var res = tr.ExecuteAsync(to).ConfigureAwait(false).GetAwaiter().GetResult();
                    if (res.Result == null)
                    {
                        model.Result = "(no record found)";
                    }
                    else
                    {
                        model.TableResult = new List<List<string>>();
                        List<string> H, R;
                        model.TableResult.Add(H = new List<string>());
                        model.TableResult.Add(R = new List<string>());
                        H.Add("PartitionKey");
                        R.Add(model.TablePartition);
                        H.Add("Key");
                        R.Add(model.TableKey);
                        if (res.Result is DynamicTableEntity dt)
                        {
                            foreach (var pp in dt.Properties)
                            {
                                H.Add(pp.Key);
                                R.Add(pp.Value.StringValue);
                            }
                            model.Result = $"{dt.Properties.Count} properties are found.";
                        }
                        else
                        {
                            var json = JsonConvert.SerializeObject(res.Result);
                            model.Result = json ?? "(null)";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"File Share Error : {ex.Message}";
            }

            model.Skip = false;
            return View(model);
        }
        public IActionResult Queue(StorageModel model)
        {
            return View(model);
        }
    }
}
