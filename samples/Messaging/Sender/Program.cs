using System;
using System.Threading;
using System.Threading.Tasks;
using FarmerConnect.Azure.Messaging;
using FarmerConnect.Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
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
                .ConfigureAppConfiguration((hostContext, configuration) =>
                {
                    configuration.AddUserSecrets("3b7f83e1-5589-40b5-9102-1757c6860b3d");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Debug));

                    //services.AddServiceBusSender(options =>
                    //{
                    //    options.ConnectionString = hostContext.Configuration["Messaging:ConnectionString"];
                    //    options.QueueName = hostContext.Configuration["Messaging:QueueName"];
                    //});

                    services.AddStorageQueueSender(options =>
                    {
                        options.ConnectionString = hostContext.Configuration["Messaging:ConnectionString"];
                        options.QueueName = hostContext.Configuration["Messaging:QueueName"];
                    });

                    services.AddHostedService<MessagingBackgroundService>();
                })
                .RunConsoleAsync();
        }
    }
    public class AcceptEvent : IntegrationEvent
    {
        public string TransactionId { get; set; }
    }

    public class MessagingBackgroundService : IHostedService
    {
        private readonly IQueueSender _queueSender;

        public MessagingBackgroundService(IQueueSender queueSender)
        {
            _queueSender = queueSender;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var @event = new AcceptEvent
                {
                    TransactionId = Guid.NewGuid().ToString()
                };
                await _queueSender.SendMessage(@event, cancellationToken);

                await Task.Delay(TimeSpan.FromMilliseconds(1400), cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
