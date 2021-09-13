using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmerConnect.Azure.Messaging.ServiceBus
{
    public class EventBusSubscriptionManager
    {
        private readonly Dictionary<string, List<Type>> _handlers = new();
        private readonly List<Type> _eventTypes = new();

        public void Subscribe<TE, TH>()
            where TE : IntegrationEvent
            where TH : IIntegrationEventHandler
        {
            var eventName = GetEventKey<TE>();

            if (!HasSubscriptionsForEvent(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            // Add event handler
            var handlerType = typeof(TH);

            if (_handlers[eventName].Any(x => x == handlerType))
            {
                throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            _handlers[eventName].Add(handlerType);

            // Add event type
            if (!_eventTypes.Contains(typeof(TE)))
            {
                _eventTypes.Add(typeof(TE));
            }
        }

        public IEnumerable<Type> GetHandlersForEvent(string eventName) => _handlers[eventName];

        public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);

        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);

        public string GetEventKey<T>()
        {
            return typeof(T).Name;
        }
    }
}
