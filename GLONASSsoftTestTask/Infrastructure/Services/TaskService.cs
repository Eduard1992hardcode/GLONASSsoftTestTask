using AutoMapper;
using AutoMapper.QueryableExtensions;
using EventBus.Abstactions;
using GLONASSsoftTestTask.Events;
using GLONASSsoftTestTask.Infrastructure.Dto;
using GLONASSsoftTestTask.Infrastructure.Models;
using GLONASSsoftTestTask.Infrastructure.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GLONASSsoftTestTask.Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        private readonly IEventBus _eventBus;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEfRepository<UserEntity> _userRepository;

        public TaskService(IEventBus eventBus,
           ApplicationDbContext context,
           IMapper mapper,
           IEfRepository<UserEntity> userRepository)
        {
            _eventBus = eventBus;
            _context = context;
            _mapper = mapper;
            _userRepository = userRepository;
        }
        public async Task<Guid> CreateStatisticTask(UserStatisticDto dto)
        {
            //TODO: проверить есть ли юзер в базе, добавить его если нет 

            var user = await _userRepository.GetById(dto.UserId);
            if (user == null)
                await _userRepository.Add(new UserEntity
                {
                     Id = dto.UserId
                });


            //получить так из дто, соранить в бд
            var task = _context.StatisticTasks.Add(_mapper.Map<StatisticTaskEntity>(dto));
            await _context.SaveChangesAsync();

            //Создать ивент, опубликовать его 
            var events = new CreateTaskStatisticEvent
            {
                TaskId = task.Entity.Id
            };
            _eventBus.Publish(events);
            
        
            return task.Entity.Id;
        }

        public async Task<InfoResponseDto> GetInfo(Guid taskGuid)
        {
            var taskResponse = await _context.StatisticTasks
                .Where(x => x.Id == taskGuid)
                .ProjectTo<InfoResponseDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            var checkResultInfo = await _context.StatisticResults
                .Include(x=>x.Task)
                .FirstOrDefaultAsync(x => x.TaskId == taskGuid);
            
            if (checkResultInfo == null)
                return taskResponse;
            
            else
            {
                var result = _mapper.Map<InfoResultResponseDto>(checkResultInfo);
                taskResponse.Result = result;
                return taskResponse;
            }
        }
    }
 }

