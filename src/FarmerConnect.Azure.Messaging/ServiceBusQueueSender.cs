﻿using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FarmerConnect.Azure.Messaging
{
    public class ServiceBusQueueSender
    {
        private readonly AzureServiceBusOptions _options;
        private readonly ServiceBusClient _client;
        private readonly ILogger<ServiceBusQueueConsumer> _logger;
        private const string INTEGRATION_EVENT_SUFFIX = "Event";

        public ServiceBusQueueSender(IOptions<AzureServiceBusOptions> options, ILogger<ServiceBusQueueConsumer> logger)
        {
            _options = options.Value;
            _logger = logger;

            _client = new ServiceBusClient(_options.ConnectionString);
        }

        public async Task SendMessage(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFFIX, "");
            var jsonMessage = JsonSerializer.Serialize(@event, @event.GetType());
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var sender = _client.CreateSender(_options.QueueName);
            await sender.SendMessageAsync(new ServiceBusMessage(body)
            {
                MessageId = Guid.NewGuid().ToString(),
                Subject = eventName
            });

            _logger.LogInformation("Successfully added message to queue: {QueueName}", _options.QueueName);
        }
    }
}