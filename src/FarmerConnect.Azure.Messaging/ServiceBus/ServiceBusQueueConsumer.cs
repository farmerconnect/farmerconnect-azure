using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FarmerConnect.Azure.Messaging.ServiceBus
{
    public class ServiceBusQueueConsumer
    {
        private readonly EventBusSubscriptionManager _subscriptionManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusOptions _options;
        private readonly ServiceBusClient _client;
        private readonly ILogger _logger;
        private ServiceBusProcessor _processor;
        private const string INTEGRATION_EVENT_SUFFIX = "Event";

        public ServiceBusQueueConsumer(EventBusSubscriptionManager subscriptionManager, IServiceProvider serviceProvider, IOptions<ServiceBusOptions> options, ILogger<ServiceBusQueueConsumer> logger)
        {
            _subscriptionManager = subscriptionManager;
            _serviceProvider = serviceProvider;
            _options = options.Value;
            _logger = logger;

            _client = new ServiceBusClient(_options.ConnectionString);
        }

        public async Task RegisterOnMessageHandlerAndReceiveMessages()
        {
            _processor = _client.CreateProcessor(_options.QueueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
            });
            _processor.ProcessMessageAsync += ProcessMessagesAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;
            await _processor.StartProcessingAsync().ConfigureAwait(false);
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Message handler encountered an exception");
            _logger.LogDebug($"- ErrorSource: {arg.ErrorSource}");
            _logger.LogDebug($"- Entity Path: {arg.EntityPath}");
            _logger.LogDebug($"- FullyQualifiedNamespace: {arg.FullyQualifiedNamespace}");

            return Task.CompletedTask;
        }

        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            var message = args.Message;

            var eventName = $"{message.Subject}{INTEGRATION_EVENT_SUFFIX}";
            var messageData = Encoding.UTF8.GetString(message.Body);

            var eventType = _subscriptionManager.GetEventTypeByName(eventName);
            var integrationEvent = JsonSerializer.Deserialize(messageData, eventType);

            // Do we have a registration for this event?
            // Get the type for the handler.
            // Create an object for it
            using (var scope = _serviceProvider.CreateScope())
            {
                var eventHandlerType = _subscriptionManager.GetHandlersForEvent(eventName);

                foreach (var handler in eventHandlerType)
                {
                    var eventHandler = scope.ServiceProvider.GetService(handler);
                    if (eventHandler == null)
                    {
                        continue;
                    }

                    var concreteType = (IIntegrationEventHandler)scope.ServiceProvider.GetService(handler);
                    await concreteType.Handle(integrationEvent).ConfigureAwait(false);
                }
            }

            await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_processor != null)
            {
                await _processor.DisposeAsync().ConfigureAwait(false);
            }

            if (_client != null)
            {
                await _client.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task CloseQueueAsync()
        {
            await _processor.CloseAsync().ConfigureAwait(false);
        }
    }
}
