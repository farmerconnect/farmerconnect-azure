using System.Threading;
using System.Threading.Tasks;

namespace FarmerConnect.Azure.Messaging
{
    public interface IQueueSender
    {
        Task SendMessageAsync(IntegrationEvent @event, CancellationToken cancellationToken = default);
    }
}
