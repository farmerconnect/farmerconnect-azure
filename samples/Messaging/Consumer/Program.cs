using System;
using System.Threading;
using System.Threading.Tasks;
using FarmerConnect.Azure.Messaging;
using FarmerConnect.Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Consumer
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

                services.AddScoped<AcceptEventHandler>();

                services.AddMessagingConsumer(options =>
                {
                    options.ConnectionString = "Endpoint=sb://tmf-tst-centralus-servicebusns.servicebus.windows.net/;SharedAccessKeyName=FarmerConnect.Consumer;SharedAccessKey=3NFZIa0EJgHbpGLJcAoXG9PkoocoILWvg1g0VtBYTJg=;EntityPath=tmf-dev-bulk-queue";
                    options.QueueName = "tmf-dev-bulk-queue";
                });

                services.AddHostedService<EventBusRegistrationBackgroundService>();
            })
            .RunConsoleAsync();
        }

        public class EventBusRegistrationBackgroundService : IHostedService
        {
            private readonly EventBusSubscriptionManager _subscriptionManager;
            private readonly ILogger<EventBusRegistrationBackgroundService> _logger;

            public EventBusRegistrationBackgroundService(EventBusSubscriptionManager subscriptionManager, ILogger<EventBusRegistrationBackgroundService> logger)
            {
                _subscriptionManager = subscriptionManager;
                _logger = logger;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                _subscriptionManager.Subscribe<AcceptEvent, AcceptEventHandler>();

                _logger.LogInformation("Completed event handler registration");

                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        public class AcceptEvent : IntegrationEvent
        {
            public string TransactionId { get; set; }
        }

        public class AcceptEventHandler : IIntegrationEventHandler
        {
            private readonly ILogger<AcceptEventHandler> _logger;

            public AcceptEventHandler(ILogger<AcceptEventHandler> logger)
            {
                _logger = logger;
            }

            public async Task Handle(object @event)
            {
                var integrationEvent = (AcceptEvent)@event;

                _logger.LogInformation("Doing the stuff for {TransactionId}...", integrationEvent.TransactionId);
                await Task.Delay(TimeSpan.FromSeconds(3));
                _logger.LogInformation("...finished the stuff.");
            }
        }
    }
}
