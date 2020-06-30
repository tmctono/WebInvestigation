// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tono;
using TonoAspNetCore;
using WebInvestigation.Models;

namespace WebInvestigation.Controllers
{
    [RequireHttps]
    public class EventHubController : Controller, IEventProcessorFactory
    {
        public List<string> ReceivedMessages { get; } = new List<string>();
        public StringBuilder ErrorMessages { get; } = new StringBuilder();

        [HttpGet]
        public IActionResult Index()
        {
            return Index(new EventHubModel
            {
                ConnectionString = EventHubModel.Default.ConnectionString,
                EventHubName = EventHubModel.Default.EventHubName,
                StorageAccountName = EventHubModel.Default.StorageAccountName,
                StorageAccountKey = EventHubModel.Default.StorageAccountKey,
                StorageContainerName = EventHubModel.Default.StorageContainerName,
                Message = $"{EventHubModel.Default.Message} at {DateTime.Now.ToString(TimeUtil.FormatYMDHMS)}",
                SkipSend = true,
            });
        }

        [HttpPost]
        public IActionResult Index(EventHubModel model)
        {
            // Apply input history from cookie
            var cu = ControllerUtils.From(this);
            cu.PersistInput("ConnectionString", model, EventHubModel.Default.ConnectionString);
            cu.PersistInput("EventHubName", model, EventHubModel.Default.EventHubName);
            cu.PersistInput("StorageAccountName", model, EventHubModel.Default.StorageAccountName);
            cu.PersistInput("StorageAccountKey", model, EventHubModel.Default.StorageAccountKey);
            cu.PersistInput("StorageContainerName", model, EventHubModel.Default.StorageContainerName);
            var receiveTimeout = model.ListeningTime - TimeSpan.FromMilliseconds(500 * 2);

            try
            {
                if (model.ReceiveRequested)
                {
                    var eph = new EventProcessorHost(
                        model.EventHubName,
                        PartitionReceiver.DefaultConsumerGroupName,
                        model.ConnectionString,
                        model.GetStorageConnectionString(),
                        model.StorageContainerName);

                    eph.RegisterEventProcessorFactoryAsync(this, new EventProcessorOptions
                    {
                        InvokeProcessorAfterReceiveTimeout = true,
                        ReceiveTimeout = receiveTimeout,
                    }
                    ).ConfigureAwait(false).GetAwaiter().GetResult();

                    Task.Delay(receiveTimeout + TimeSpan.FromMilliseconds(500)).ConfigureAwait(false).GetAwaiter().GetResult();    // wait message received

                    eph.UnregisterEventProcessorAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                    model.ActionMessage = "";
                    var err = ErrorMessages.ToString();
                    if (!string.IsNullOrEmpty(err))
                    {
                        model.ActionMessage = $"{err}{Environment.NewLine}{Environment.NewLine}";
                    }
                    model.ActionMessage += $"Received {ReceivedMessages.Count} messages.{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, ReceivedMessages)}";

                    Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false).GetAwaiter().GetResult();    // wait message received
                }
                else
                if (!model.SkipSend)
                {
                    var cs = new EventHubsConnectionStringBuilder(model.ConnectionString)
                    {
                        EntityPath = model.EventHubName,
                    };
                    var ec = EventHubClient.CreateFromConnectionString(cs.ToString()); // WARNING : Max # of TCP socket because of each instance created by Http Request
                    ec.SendAsync(new EventData(Encoding.UTF8.GetBytes(model.Message))).ConfigureAwait(false).GetAwaiter().GetResult();
                    model.ActionMessage = $"OK : Sent '{model.Message}' to {model.EventHubName}";
                }
            }
            catch (Exception ex)
            {
                model.ActionMessage = $"ERROR : {ex.Message}";
            }

            model.ReceiveRequested = false;
            model.SkipSend = false;

            return View(model);
        }
        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new MyEventProcessor
            {
                Controller = this,
            };
        }
    }


    public class MyEventProcessor : IEventProcessor
    {
        public EventHubController Controller { get; set; }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            Controller.ErrorMessages.AppendLine($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
            return Task.CompletedTask;
        }
        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var eventData in messages)
            {
                var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                Controller.ReceivedMessages.Add(data);
            }
            return context.CheckpointAsync();
        }
    }
}
