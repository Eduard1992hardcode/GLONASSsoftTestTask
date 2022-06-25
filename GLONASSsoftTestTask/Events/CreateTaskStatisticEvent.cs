using EventBus.Events;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GLONASSsoftTestTask.Events
{
    public class CreateTaskStatisticEvent : IntegrationEvent
    {
        public Guid TaskId { get; set; }
    }
}
