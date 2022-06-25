using System;

namespace GLONASSsoftTestTask.Infrastructure.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetExceptionMessage(this Exception exception)
        {
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            return exception.Message;
        }
    }
}
