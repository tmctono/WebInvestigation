using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WebInvestigation.Models
{
    public class KeyVaultModel
    {
        public static readonly KeyVaultModel Default = new KeyVaultModel
        {
            Url = "https://<your keyvault name>.azure.net/",
            Key = "secret key name here",
            ApplicationID = "ZZZZZZZZ-ZZZZ-ZZZZ-ZZZZ-ZZZZZZZZZZZZ",
            ClientSecret = "1234567890123456789012345678901234",
        };
        public string ApplicationID { get; set; }
        public string ClientSecret { get; set; }
        public string Url { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string ErrorMessage { get; set; }
        public bool SkipRequest { get; set; }
    }
}
