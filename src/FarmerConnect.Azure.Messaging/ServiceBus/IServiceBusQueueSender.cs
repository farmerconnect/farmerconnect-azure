using System.Threading;
using System.Threading.Tasks;

namespace FarmerConnect.Azure.Messaging.ServiceBus
{
    public interface IServiceBusQueueSender
    {
        Task SendMessage(IntegrationEvent @event, CancellationToken cancellationToken = default);
    }
}
