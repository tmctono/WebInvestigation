// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using DotNetty.Common.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebInvestigation.Models;

namespace WebInvestigation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        private static readonly HttpClient HTTP = new HttpClient();

        public IActionResult Ping(PingModel model = null)
        {
            model ??= new PingModel();
            if (!string.IsNullOrEmpty(model.Ip))
            {
                var res = HTTP.GetAsync($"{Request.Scheme}://{Request.Host}/api/Ping?ip={model.Ip}");
                model.Set(res);
            }
            return View(model);
        }

        public IActionResult HttpStatus(HttpStatus status)
        {
            return StatusCode(status?.Code ?? 200);
        }

        public IActionResult ThrowException()
        {
            throw new System.Exception($"This is a TEST Exception thrown by user at {DateTime.UtcNow}");
        }

        public IActionResult StackOverFlow()
        {
            StackOverFlow();
            return View();
        }

        public IActionResult Cpu()
        {
            var cts = new CancellationTokenSource(10000);
            for (var ii = 0; !cts.IsCancellationRequested; ii++)
            {
                Task.Run(() =>
                {
                    var rnd = new Random();
                    var a = "";
                    for( var i = 0; !cts.IsCancellationRequested; i++)
                    {
                        a += ((char)(rnd.NextDouble() * 25 + 'A'));
                        if (a.Length > 26)
                        {
                            a = a.Substring(1, a.Length - 1);
                        }
                    }
                });
            }
            return View();
        }

        public IActionResult Dns(DnsModel model = null)
        {
            model ??= new DnsModel();
            if (!string.IsNullOrEmpty(model.Host))
            {
                var res = HTTP.GetAsync($"{Request.Scheme}://{Request.Host}/api/Dns?host={model.Host}");
                model.Set(res);
            }
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
