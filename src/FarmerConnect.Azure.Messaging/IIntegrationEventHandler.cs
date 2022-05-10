using System.Threading.Tasks;

namespace FarmerConnect.Azure.Messaging
{
    public interface IIntegrationEventHandler
    {
        public Task Handle(object @event);
    }
}
