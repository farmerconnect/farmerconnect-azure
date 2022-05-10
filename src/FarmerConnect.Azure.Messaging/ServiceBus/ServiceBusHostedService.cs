using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FarmerConnect.Azure.Messaging.ServiceBus
{
    public class ServiceBusHostedService : IHostedService, IDisposable
    {
        private bool _disposedValue;
        private readonly ILogger<ServiceBusHostedService> _logger;
        private readonly ServiceBusQueueConsumer _serviceBusQueueConsumer;

        public ServiceBusHostedService(ServiceBusQueueConsumer serviceBusQueueConsumer, ILogger<ServiceBusHostedService> logger)
        {
            _serviceBusQueueConsumer = serviceBusQueueConsumer;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Starting the service bus queue consumer");
            await _serviceBusQueueConsumer.RegisterOnMessageHandlerAndReceiveMessages().ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Stopping the service bus queue consumer");
            await _serviceBusQueueConsumer.CloseQueueAsync().ConfigureAwait(false);
        }

        protected virtual async void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    await _serviceBusQueueConsumer.DisposeAsync().ConfigureAwait(false);
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
