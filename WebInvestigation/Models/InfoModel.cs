using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tono;

namespace WebInvestigation.Models
{
    public class InfoModel
    {
        public HttpRequest Request { get; set; }
        public HttpResponse Response { get; set; }

        public decimal DecimalSample = 100;
        public string TEMP => Environment.GetEnvironmentVariable("TEMP");
        public string HOME => Environment.GetEnvironmentVariable("HOME");

        public string GetIPs()
        {
            string ret = "";
            foreach (var adr in Dns.GetHostAddresses(Dns.GetHostName()).Distinct())
            {
                if (ret != "")
                {
                    ret = ret + ", ";
                }
                ret = ret + adr;
            }
            if (ret == "")
            {
                return null;
            }
            else
            {
                return ret;
            }
        }

        static HttpClient HTTP = new HttpClient();

        public string GetOutboundIP()
        {
            var restask = HTTP.GetAsync($"https://aqtono.com/ci.php?dummy={MathUtil.Rand(10000000, 99999999)}");
            restask.Wait();
            var res = restask.Result;
            var iptm = res.Content.ReadAsStringAsync().Result;
            return iptm;
        }

        public string Format(object s)
        {
            if (s == null)
            {
                return "<i>(null)</i>";
            }
            var ret = s.ToString();
            if (string.IsNullOrEmpty(ret))
            {
                return "<i>(empty)</i>";
            }
            ret = ret.Replace("<script>", "＜ｓｃｒｉｐｔ＞");
            var cs = ret.Split(';');
            if (cs.Length > 1)
            {
                ret = "";
                for (var i = 0; i < cs.Length; i++)
                {
                    var line = cs[i];
                    var co = line.Split('=');
                    if (co.Length > 1)
                    {
                        ret += $"<strong>{co[0]} = </strong>" + string.Join("=", co.Skip(1));
                    }
                    else
                    {
                        ret += line;
                    }
                    if (i < cs.Length - 1)
                    {
                        ret += "<strong>;</strong>";
                    }
                    ret += "<br>";
                }
            }
            return ret;
        }
    }
}
