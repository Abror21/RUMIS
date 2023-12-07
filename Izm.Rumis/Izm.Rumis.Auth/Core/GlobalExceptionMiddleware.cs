using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Izm.Rumis.Auth.Core
{
    public class GlobalExceptionMiddleware
    {
        private readonly AppSettings options;
        private readonly RequestDelegate next;
        private readonly ILogger<GlobalExceptionMiddleware> logger;

        public GlobalExceptionMiddleware(RequestDelegate next, IOptions<AppSettings> options, ILogger<GlobalExceptionMiddleware> logger)
        {
            this.next = next;
            this.options = options.Value;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            logger.LogError(exception, exception.Message);

            if (exception.InnerException != null)
                logger.LogError(exception.InnerException, exception.InnerException.Message);

            //return context.Response.WriteAsync($"An unexpected error ({context.TraceIdentifier}) occured. Please contact the administrator.");
            context.Response.Redirect(options.ErrorRedirectUrl);

            return Task.CompletedTask;
        }
    }
}
