// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Collections.Generic;
using System.Linq;
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
                StorageConnectionString = EventHubModel.Default.StorageConnectionString,
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
            cu.PersistInput("ConsumerGroupName", model, EventHubModel.Default.ConsumerGroupName);
            cu.PersistInput("StorageConnectionString", model, EventHubModel.Default.StorageConnectionString);
            cu.PersistInput("StorageContainerName", model, EventHubModel.Default.StorageContainerName);

            var receiveTimeout = model.ListeningTime - TimeSpan.FromMilliseconds(200 * 2);
            if( model.ConsumerGroupName == EventHubModel.Default.ConsumerGroupName)
            {
                model.ConsumerGroupName = PartitionReceiver.DefaultConsumerGroupName;
            }

            try
            {
                if (model.ReceiveRequested)
                {
                    var eph = new EventProcessorHost(
                        model.EventHubName,
                        model.ConsumerGroupName,
                        model.ConnectionString,
                        model.StorageConnectionString,
                        model.StorageContainerName);

                    eph.RegisterEventProcessorFactoryAsync(this, new EventProcessorOptions
                    {
                        InvokeProcessorAfterReceiveTimeout = true,
                        ReceiveTimeout = receiveTimeout,    // It's necessary to set long time here. (1.6sec was not enough at 2020.7.30)
                    }
                    ).ConfigureAwait(false).GetAwaiter().GetResult();

                    Task.Delay(receiveTimeout + TimeSpan.FromMilliseconds(200)).ConfigureAwait(false).GetAwaiter().GetResult();    // wait message received

                    eph.UnregisterEventProcessorAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                    model.ActionMessage = "";
                    var err = ErrorMessages.ToString();
                    if (!string.IsNullOrEmpty(err))
                    {
                        model.ActionMessage = $"{err}{Environment.NewLine}{Environment.NewLine}";
                    }
                    model.ActionMessage += $"Received {ReceivedMessages.Count} messages.{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, ReceivedMessages)}";

                    Task.Delay(TimeSpan.FromMilliseconds(200)).ConfigureAwait(false).GetAwaiter().GetResult();    // wait message received
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

                    // Collect Partition Information
                    for ( var i = 0; i < 32; i++)
                    {
                        try
                        {
                            if( model.PartitionInfo == null)
                            {
                                model.PartitionInfo = new Dictionary<int, EventHubPartitionRuntimeInformation>();
                            }
                            model.PartitionInfo[i] = ec.GetPartitionRuntimeInformationAsync($"{i}").ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                        catch
                        {
                            model.PartitionInfo[i] = null;
                        }
                    }
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
