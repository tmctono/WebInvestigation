using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebInvestigation.Models;

namespace WebInvestigation.Controllers
{
    [RequireHttps]
    public class InfoController : Controller
    {
        public IActionResult Index()
        {
            return View(new InfoModel
            {
                Request = Request,
                Response = Response,
            });
        }
    }
}
