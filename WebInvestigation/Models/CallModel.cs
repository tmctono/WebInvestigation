using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WebInvestigation.Models
{
    public class CallModel : ResponseBase
    {
        public static readonly CallModel Default = new CallModel
        {
            Uri = $"https://ntp-a1.nict.go.jp/cgi-bin/json?a=1&b=2",
            Method = "(unknown)",
            Body = "c=3&d=4",
        };
        public string Uri { get; set; }
        public string Method { get; set; }
        public string Body { get; set; }
        public bool SkipCall { get; set; }
        public string SampleUri { get; set; }

        public string[] GetMethodList()
        {
            var t = typeof(HttpMethod);
            var ms = t.GetProperties(System.Reflection.BindingFlags.Static| System.Reflection.BindingFlags.Public);
            var ret = ms.Select(a => a.Name.ToUpper()).OrderBy(a => a).ToArray();
            return ret;
        }
    }
}
