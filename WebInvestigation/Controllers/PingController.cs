// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;
using System.Text;

namespace WebInvestigation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    {
        public string Get(string ip = "127.0.0.1")
        {
            var ret = new StringBuilder();
            ret.AppendLine($"Ping to {ip ?? "(null)"}");

            var sender = new Ping();
            var reply = sender.Send(ip);
            if (reply.Status == IPStatus.Success)
            {
                ret.AppendLine($"Reply from {reply.Address}: bytes={reply.Buffer.Length} time={reply.RoundtripTime}[ms] TTL={reply.Options?.Ttl}");
            }
            else
            {
                ret.AppendLine(reply.Status.ToString() ?? "(n/a)");
            }
            return ret.ToString();
        }
    }
}
