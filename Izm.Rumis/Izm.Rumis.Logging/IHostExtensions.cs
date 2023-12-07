using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;

namespace Izm.Rumis.Logging
{
    public static class IHostExtensions
    {
        public static IHost ConfigureLogging(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var connectionString = Environment.GetEnvironmentVariable(EnvironmentVariable.DatabaseConnectionString)
                ?? config.GetConnectionString("Default")
                ?? throw new ConfigurationErrorsException("Database connection string not found.");

            MySqlLog4netAppender.SetDatabase(connectionString);

            return host;
        }

        public static IHost ConfigureWebLogging(this IHost host)
        {
            ConfigureLogging(host);

            using (var scope = host.Services.CreateScope())
            {
                var httpCtx = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

                log4net.GlobalContext.Properties["ip"] = new HttpContextPropertyProvider(httpCtx, ctx =>
                {
                    string result = ctx.Connection.RemoteIpAddress.ToString();

                    if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues ipAddress))
                    {
                        var ip = ipAddress.FirstOrDefault();

                        if (!string.IsNullOrEmpty(ip))
                        {
                            result = ip.Split(',').First();
                        }
                    }

                    return result;
                });

                log4net.GlobalContext.Properties["path"] = new HttpContextPropertyProvider(httpCtx, ctx => ctx.Request.Path.ToString() + ctx.Request.QueryString.ToString());
                log4net.GlobalContext.Properties["method"] = new HttpContextPropertyProvider(httpCtx, ctx => ctx.Request.Method);
                log4net.GlobalContext.Properties["useragent"] = new HttpContextPropertyProvider(httpCtx, ctx => ctx.Request.Headers["User-Agent"].ToString());
                log4net.GlobalContext.Properties["username"] = new HttpContextPropertyProvider(httpCtx, ctx => ctx.User?.FindFirst(ClaimTypes.Name)?.Value);
                log4net.GlobalContext.Properties["traceid"] = new HttpContextPropertyProvider(httpCtx, ctx => ctx.TraceIdentifier);
            }

            return host;
        }
    }
}
