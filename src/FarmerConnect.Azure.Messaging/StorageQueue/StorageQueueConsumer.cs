using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Azure.Storage.Queues;
using System.Text.Json;
using System;

namespace FarmerConnect.Azure.Messaging.StorageQueue
{
    public class StorageQueueConsumer : BackgroundService
    {
        private readonly EventSubscriptionManager _subscriptionManager;
        private readonly MessagingOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StorageQueueConsumer> _logger;

        private const int MaxPollingInterval = 51200; // This is the maximum polling interval. Since we pay connections to the storage account this could lead to unwanted cost if not high enough.
        private const int MaxMessages = 1; // Maximum number of messages to receive at once. If set to one then the behaviour is more similar to the service bus implementation with pub / sub.

        public StorageQueueConsumer(EventSubscriptionManager subscriptionManager, IOptionsSnapshot<MessagingOptions> options, IServiceProvider serviceProvider, ILogger<StorageQueueConsumer> logger)
        {
            _subscriptionManager = subscriptionManager;
            _options = options.Value;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queue = new QueueClient(_options.ConnectionString, _options.QueueName);
            await queue.CreateIfNotExistsAsync(cancellationToken: stoppingToken);

            var waitTime = 100;

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("Checking for new messages...");

                // When a message is found, the runtime waits 100 milliseconds and then checks for another message
                // When no message is found, it waits about 200 milliseconds before trying again.
                // After subsequent failed attempts to get a queue message, the wait time continues to increase until it reaches the maximum wait time, which defaults to one minute.
                // The maximum wait time is configurable via the maxPollingInterval property in the host.json file.

                var messages = queue.ReceiveMessages(MaxMessages, cancellationToken: stoppingToken).Value;
                if (messages.Length > 0)
                {
                    _logger.LogDebug("We have some new messages, let's process them");
                    waitTime = 100;

                    foreach (var message in messages)
                    {
                        try
                        {
                            var jsonDocument = JsonDocument.Parse(message.Body);

                            var eventName = jsonDocument.RootElement.GetProperty("eventName").GetString();
                            var eventType = _subscriptionManager.GetEventTypeByName(eventName);
                            var messageData = jsonDocument.RootElement.GetProperty("data").GetString();

                            var integrationEvent = JsonSerializer.Deserialize(messageData, eventType, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var eventHandlerTypes = _subscriptionManager.GetHandlersForEvent(eventName);

                                foreach (var handler in eventHandlerTypes)
                                {
                                    var eventHandler = scope.ServiceProvider.GetService(handler);
                                    if (eventHandler == null)
                                    {
                                        _logger.LogWarning("Unable to find registered service for: {EventHandler}", handler.Name);
                                        continue;
                                    }

                                    var concreteType = (IIntegrationEventHandler)scope.ServiceProvider.GetService(handler);
                                    await concreteType.Handle(integrationEvent).ConfigureAwait(false);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Unable to process message: {MessageBody}", message.Body);
                        }
                        finally
                        {
                            await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt, stoppingToken);
                        }
                    }
                }
                else
                {
                    if (waitTime < MaxPollingInterval)
                    {
                        waitTime *= 2;
                    }
                    _logger.LogDebug("No new message was received. Increasing the polling wait timer to {WaitTimeInMs}ms", waitTime);
                    await Task.Delay(waitTime, stoppingToken);
                }
            }
        }
    }
}
