using System.Threading.Tasks;

namespace FarmerConnect.Azure.Messaging.ServiceBus
{
    public interface IIntegrationEventHandler
    {
        public Task Handle(object @event);
    }
}
