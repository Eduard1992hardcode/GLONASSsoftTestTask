using EventBus.Abstactions;
using EventBus.Events;
using Microsoft.Extensions.DependencyInjection;


namespace EventBusRabbitMQ
{
    public class EventBusSubscriptionsBuilder
    {
        private readonly IServiceCollection _services;

        internal List<Action<IEventBusSubscriptionsManager>> Subscriptions { get; }
            = new List<Action<IEventBusSubscriptionsManager>>();

        internal void AddSubscriptions(IEventBusSubscriptionsManager manager)
        {
            foreach (var subscription in Subscriptions)
            {
                subscription(manager);
            }
        }

        public EventBusSubscriptionsBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public EventBusSubscriptionsBuilder Subscribe<TEvent, TEventHandler>()
            where TEvent : IntegrationEvent
            where TEventHandler : class, IIntegrationEventHandler<TEvent>
        {
            _services.AddTransient<TEventHandler>();
            Subscriptions.Add(m => m.AddSubscription<TEvent, TEventHandler>());
            return this;
        }
    }

    internal class SubscriptionInfo
    {
        public Type EventType { get; set; }

        public Type EventHandlerType { get; set; }
    }
}
