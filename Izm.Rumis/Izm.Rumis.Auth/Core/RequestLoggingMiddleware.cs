using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Izm.Rumis.Auth.Core
{
    public class RequestLoggingMiddleware
    {
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        private readonly RequestDelegate next;
        private readonly ILogger<RequestLoggingMiddleware> logger;

        public async Task Invoke(HttpContext context)
        {
            var watch = Stopwatch.StartNew();

            context.Response.OnStarting(() =>
            {
                watch.Stop();
                logger.LogInformation("Process request: {statusCode}, completed in {duration}ms.", context.Response?.StatusCode, watch.ElapsedMilliseconds);

                return Task.CompletedTask;
            });

            await next(context);
        }
    }
}
