using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace WebInvestigation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DnsController : ControllerBase
    {
        public string Get(string host)
        {
            if (string.IsNullOrEmpty(host))
            {
                return "ERROR : not found host parameter";
            }
            else
            {
                try
                {
                    var ret = System.Net.Dns.GetHostEntry(host);
                    var message = string.Join(", ", ret.AddressList.Select(a => a.ToString()));
                    return message;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
    }
}
