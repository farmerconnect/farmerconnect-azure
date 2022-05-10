using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FarmerConnect.Azure.Messaging.StorageQueue
{
    public class StorageQueueSender : IQueueSender
    {
        private readonly ILogger<StorageQueueSender> _logger;
        private readonly MessagingOptions _options;

        public StorageQueueSender(IOptions<MessagingOptions> options, ILogger<StorageQueueSender> logger)
        {
            _logger = logger;
            _options = options.Value;
        }

        public async Task SendMessageAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            var queue = new QueueClient(_options.ConnectionString, _options.QueueName);
            await queue.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var eventName = @event.GetType().Name;

            var message = JsonSerializer.Serialize(new
            {
                EventName = eventName,
                Data = JsonSerializer.Serialize(@event, @event.GetType(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await queue.SendMessageAsync(message, cancellationToken);

            _logger.LogDebug("Successfully added message to queue: {QueueName}", _options.QueueName);
        }
    }
}
