using EventBus.Events;
using System.Threading.Tasks;

namespace EventBus.Abstactions
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
         where TIntegrationEvent : IntegrationEvent
    {
        Task<bool> Handle(TIntegrationEvent @event);
    }

    public interface IIntegrationEventHandler
    {
    }
}
