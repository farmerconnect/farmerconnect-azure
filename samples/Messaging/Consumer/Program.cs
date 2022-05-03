using System;
using System.Threading;
using System.Threading.Tasks;
using FarmerConnect.Azure.Messaging;
using Microsoft.Extensions.Configuration;
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
                .ConfigureAppConfiguration((hostContext, configuration) =>
                {
                    configuration.AddUserSecrets("3b7f83e1-5589-40b5-9102-1757c6860b3d");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Debug));

                    services.AddScoped<AcceptEventHandler>();
                    services.AddScoped<SecondAcceptEventHandler>();

                    //services.AddServiceBusConsumer(options =>
                    //{
                    //    options.ConnectionString = hostContext.Configuration["Messaging:ConnectionString"];
                    //    options.QueueName = hostContext.Configuration["Messaging:QueueName"];
                    //});

                    services.AddStorageQueueConsumer(options =>
                    {
                        options.ConnectionString = hostContext.Configuration["Messaging:ConnectionString"];
                        options.QueueName = hostContext.Configuration["Messaging:QueueName"];
                    });

                    services.AddHostedService<EventRegistrationBackgroundService>();
                })
                .RunConsoleAsync();
        }

        public class EventRegistrationBackgroundService : IHostedService
        {
            private readonly EventSubscriptionManager _subscriptionManager;
            private readonly ILogger<EventRegistrationBackgroundService> _logger;

            public EventRegistrationBackgroundService(EventSubscriptionManager subscriptionManager, ILogger<EventRegistrationBackgroundService> logger)
            {
                _subscriptionManager = subscriptionManager;
                _logger = logger;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                _subscriptionManager.Subscribe<AcceptEvent, AcceptEventHandler>();
                _subscriptionManager.Subscribe<AcceptEvent, SecondAcceptEventHandler>();

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

        public class SecondAcceptEventHandler : IIntegrationEventHandler
        {
            private readonly ILogger<SecondAcceptEventHandler> _logger;

            public SecondAcceptEventHandler(ILogger<SecondAcceptEventHandler> logger)
            {
                _logger = logger;
            }

            public async Task Handle(object @event)
            {
                var integrationEvent = (AcceptEvent)@event;

                _logger.LogInformation("Doing other stuff for {TransactionId}...", integrationEvent.TransactionId);
                await Task.Delay(TimeSpan.FromSeconds(1));
                _logger.LogInformation("...finished the other stuff.");
            }
        }
    }
}
