using EventBus.Events;
using System.Collections.Generic;

namespace EventBus.Abstactions
{
    public interface IEventBus
    {
        void Publish(IntegrationEvent @event);

        void Publish(IEnumerable<IntegrationEvent> events);

        void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;

        void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;

        void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent;
    }
}
