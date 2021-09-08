using System;
using System.Threading;
using System.Threading.Tasks;
using FarmerConnect.Azure.Messaging;
using FarmerConnect.Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sender
{
    /// <summary>
    ///  NOTE: To make it easier to follow the sample, all classes are in this single file.
    ///  Please do not follow this pattern and create separate files for each class.
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging(configure => configure.AddConsole());

                services.AddMessagingSender(options =>
                {
                    options.ConnectionString = "Endpoint=sb://tmf-tst-centralus-servicebusns.servicebus.windows.net/;SharedAccessKeyName=FarmerConnect.Consumer;SharedAccessKey=3NFZIa0EJgHbpGLJcAoXG9PkoocoILWvg1g0VtBYTJg=;EntityPath=tmf-dev-bulk-queue";
                    options.QueueName = "tmf-dev-bulk-queue";
                });

                services.AddHostedService<EventBusSenderBackgroundService>();
            })
            .RunConsoleAsync();
        }
    }
    public class AcceptEvent : IntegrationEvent
    {
        public string TransactionId { get; set; }
    }

    public class EventBusSenderBackgroundService : IHostedService
    {
        private readonly ServiceBusQueueSender _serviceBusQueueSender;
        private readonly ILogger<EventBusSenderBackgroundService> _logger;

        public EventBusSenderBackgroundService(ServiceBusQueueSender serviceBusQueueSender, ILogger<EventBusSenderBackgroundService> logger)
        {
            _serviceBusQueueSender = serviceBusQueueSender;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var @event = new AcceptEvent
                {
                    TransactionId = Guid.NewGuid().ToString()
                };
                await _serviceBusQueueSender.SendMessage(@event, cancellationToken);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
