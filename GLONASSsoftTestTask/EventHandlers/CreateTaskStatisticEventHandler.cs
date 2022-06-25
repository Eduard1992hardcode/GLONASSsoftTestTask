using EventBus.Abstactions;
using GLONASSsoftTestTask.Events;
using GLONASSsoftTestTask.Infrastructure.Models;
using GLONASSsoftTestTask.Infrastructure.Models.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GLONASSsoftTestTask.EventHandlers
{
    public class CreateTaskStatisticEventHandler : IIntegrationEventHandler<CreateTaskStatisticEvent>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateTaskStatisticEventHandler> _logger;
        private decimal _percentageOfAMinute = 1.666666666666667M;

        public CreateTaskStatisticEventHandler(ApplicationDbContext context,
            ILogger<CreateTaskStatisticEventHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<bool> Handle(CreateTaskStatisticEvent @event)
        {
            var task =(object) _context.StatisticTasks.First(x => x.Id == @event.Id);

            TimerCallback tm = new(Count);
            Timer timer = new(tm, task, 600, 60000);

            
            throw new System.NotImplementedException();
        }
        private void Count(object obj)
        {
            var task = (StatisticTaskEntity)obj;
            task.Percent += _percentageOfAMinute;

            var diff = Math.Abs(task.Percent - 100);
            if ((double)diff < 0.0000001)
            {
                var random = new Random();
                task.Result = new StatisticTaskResultEntity
                {
                    CountSignIn = random.Next(0, 100),
                    Id = Guid.NewGuid(),
                    TaskId = task.Id
                };
                _context.StatisticTasks.Update(task);
            }

        }
    }
}
