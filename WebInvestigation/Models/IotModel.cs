using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebInvestigation.Models
{
    public class IotModel
    {
        public static readonly IotModel Default = new IotModel
        {
            DeviceId = "<your device id>",
            DeviceKey = "<your device shared key>",
        };
        public string DeviceId { get; set; }
        public string DeviceKey { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}
