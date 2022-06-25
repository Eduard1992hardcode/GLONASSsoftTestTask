using GLONASSsoftTestTask.Infrastructure.Dto;
using GLONASSsoftTestTask.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GLONASSsoftTestTask.Controllers
{
    [Route("[controller]/[action]")]
    public class ReportController : Controller
    {
        private readonly ITaskService _taskService;

        public ReportController(ITaskService taskService)
        {
            _taskService = taskService;        
        }

        [HttpPost]
        public async Task<IActionResult> UserStatistics([FromBody] UserStatisticDto dto)
        {
            try
            {
                return Ok(await _taskService.CreateStatisticTask(dto));
            }
            catch (Exception e)
            {

               return BadRequest(e.Message);
            }
            
        }
        
        [HttpGet("{guid}")]
        public string Info(Guid guid)
        {
            return "value";
        }

    }
}
