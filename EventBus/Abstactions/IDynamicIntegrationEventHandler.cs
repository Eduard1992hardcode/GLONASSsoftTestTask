using System.Threading.Tasks;

namespace EventBus.Abstactions
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}
