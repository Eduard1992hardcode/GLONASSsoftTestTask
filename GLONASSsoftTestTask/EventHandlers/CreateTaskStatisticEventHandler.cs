using EventBus.Abstactions;
using GLONASSsoftTestTask.Events;
using GLONASSsoftTestTask.Infrastructure.Models;
using GLONASSsoftTestTask.Infrastructure.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GLONASSsoftTestTask.EventHandlers
{
    public class CreateTaskStatisticEventHandler : IIntegrationEventHandler<CreateTaskStatisticEvent>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CreateTaskStatisticEventHandler> _logger;
        private decimal _percentageOfAMinute = 1.666666666666667M;
        private Timer _timer;

        public CreateTaskStatisticEventHandler(IServiceScopeFactory scopeFactory,
            ILogger<CreateTaskStatisticEventHandler> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<bool> Handle(CreateTaskStatisticEvent @event)
        {
            var taskId = (object) @event.TaskId;
            TimerCallback tm = new(Calculate);
            _timer = new(tm, taskId, 1, 1000);
            
            return true;

        }
        private void Calculate(object obj)
        {
            var taskId = (Guid)obj;
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var task = dbContext.StatisticTasks
                .First(x => x.Id == taskId);
            


            var diff = Math.Abs(task.Percent - 100);
            if ((double)diff < 0.0000001)
            {
                var random = new Random();
                var result = new StatisticTaskResultEntity
                {
                    CountSignIn = random.Next(0, 100),
                    Id = Guid.NewGuid(),
                    TaskId = task.Id
                };
                dbContext.StatisticResults.Add(result);
                dbContext.SaveChanges();
                _timer.Dispose();
            }
            else 
            {
                task.Percent += _percentageOfAMinute;
                dbContext.StatisticTasks.Update(task);
                dbContext.SaveChanges();
            }
        }
    }
}
