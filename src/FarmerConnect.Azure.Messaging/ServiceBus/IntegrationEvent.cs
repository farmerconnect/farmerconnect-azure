using System;
using System.Text.Json.Serialization;

namespace FarmerConnect.Azure.Messaging.ServiceBus
{
    public class IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        [JsonConstructor]
        public IntegrationEvent(Guid id, DateTime createDate)
        {
            Id = id;
            CreationDate = createDate;
        }

        public Guid Id { get; private set; }

        public DateTime CreationDate { get; private set; }
    }
}
