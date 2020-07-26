// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices.Client;
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
            return Index(new IotModel
            {
                HostName = IotModel.Default.HostName,
                DeviceId = IotModel.Default.DeviceId,
                DeviceKey = IotModel.Default.DeviceKey,
                Message = $"IoT Test Message at ",
            });
        }

        [HttpPost]
        public IActionResult Index(IotModel model)
        {
            var cu = ControllerUtils.From(this);
            cu.PersistInput("HostName", model, IotModel.Default.HostName);
            cu.PersistInput("DeviceId", model, IotModel.Default.DeviceId);
            cu.PersistInput("DeviceKey", model, IotModel.Default.DeviceKey);

            if (model.Message.StartsWith("IoT Test Message at "))
            {
                model.Message = $"IoT Test Message at {DateTime.UtcNow.ToString(Tono.TimeUtil.FormatYMDHMS)}";
            }

            // Send Message to IoT Hub
            try
            {
                var client = DeviceClient.Create(model.HostName, new DeviceAuthenticationWithRegistrySymmetricKey(model.DeviceId, model.DeviceKey), model.TransportType);
                var mes = new Message(Encoding.UTF8.GetBytes(model.Message));
                client.SendEventAsync(
                    message: mes,
                    cancellationToken: new CancellationTokenSource(TimeSpan.FromMilliseconds(5000)).Token
                ).ConfigureAwait(false).GetAwaiter().GetResult();

                model.Result = $"Successfully sent the message '{model.Message}' from Device [{model.DeviceId}]";
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
            }

            return View(model);
        }
    }
}

