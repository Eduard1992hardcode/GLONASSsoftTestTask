using GLONASSsoftTestTask.Infrastructure.Dto;
using System;
using System.Threading.Tasks;

namespace GLONASSsoftTestTask.Infrastructure.Services
{
    public interface ITaskService
    {
        Task<Guid> CreateStatisticTask(UserStatisticDto dto);

        Task<InfoResponseDto> GetInfo(Guid taskGuid);
    }
}
