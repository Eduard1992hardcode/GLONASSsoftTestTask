using GLONASSsoftTestTask.Infrastructure.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace GLONASSsoftTestTask.Infrastructure.Filters
{
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<HttpGlobalExceptionFilter> _logger;
        private readonly IWebHostEnvironment _environment;

        public HttpGlobalExceptionFilter(ILogger<HttpGlobalExceptionFilter> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(new EventId(context.Exception.HResult),
                context.Exception,
                context.Exception.GetExceptionMessage());

            if (_environment.IsDevelopment())
            {
                context.Result = new BadRequestObjectResult(new string[] { context.Exception.ToString() });
            }
            else
            {
                context.Result = new BadRequestObjectResult(new string[] { "Произошла ошибка, попробуйте позже" });
            }

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.ExceptionHandled = true;
        }
    }
}
