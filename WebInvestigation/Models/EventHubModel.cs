using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebInvestigation.Models
{
    public class EventHubModel
    {
        public static readonly EventHubModel Default = new EventHubModel
        {
            ConnectionString = "Endpoint=sb://<your event hub name>.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=12345678901234567890123+123456789=",
            EventHubName = "(Your Event Hub Name)",
            StorageAccountName = "(Your Storaget Account Name)",
            StorageAccountKey = "(Your Storage Account Key)",
            StorageContainerName = "(Your Storage Container Name)",

            Message = "(Your message to send)",
        };
        public string ConnectionString { get; set; }
        public string EventHubName { get; set; }

        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public string StorageContainerName { get; set; }
        public string GetStorageConnectionString() => $"DefaultEndpointsProtocol=https;AccountName={StorageAccountName};AccountKey={StorageAccountKey}";

        public string Message { get; set; }

        public string ActionMessage { get; set; }
        public bool SkipSend { get; set; }
        public bool ReceiveRequested { get; set; }
    }
}
