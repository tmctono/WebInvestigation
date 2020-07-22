// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TonoAspNetCore;
using WebInvestigation.Models;

namespace WebInvestigation.Controllers
{
    [RequireHttps]
    public class IotController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new IotModel
            {
                DeviceId = IotModel.Default.DeviceId,
                DeviceKey = IotModel.Default.DeviceKey,
            });
        }

        [HttpPost]
        public IActionResult Index(IotModel model)
        {
            var cu = ControllerUtils.From(this);
            cu.PersistInput("DeviceId", model, IotModel.Default.DeviceId);
            cu.PersistInput("DeviceKey", model, IotModel.Default.DeviceKey);

            return View(model);
        }
    }
}

