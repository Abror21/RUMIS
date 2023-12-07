using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure;
using Izm.Rumis.Tasks.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Tasks.Services
{
    public sealed class ClearLogsTaskService : TaskService
    {
        public override string ParameterCodeEnabled => ParameterCode.ClearLogsTaskEnabled;
        public override string ParameterCodeIntervalInMinutes => ParameterCode.ClearLogsTaskIntervalInMinutes;
        public override string ParameterCodeStartTime => ParameterCode.ClearLogsTaskStartTime;

        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<ClearLogsTaskService> logger;

        public ClearLogsTaskService(
            IServiceScopeFactory serviceScopeFactory, 
            ILogger<ClearLogsTaskService> logger,
            HostedServiceTimer timer
            ) : base(serviceScopeFactory, logger, timer)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Start task.");

            using var scope = serviceScopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var commandTimeoutInHours = 1;

            var logValidInDays = int.Parse((await db.Parameters.FirstAsync(t => t.Code == ParameterCode.ClearLogsTaskLogValidInDays, cancellationToken)).Value);

            int retryCount = 0;

            while (retryCount < 3)
            {
                try
                {
                    db.Database.SetCommandTimeout(TimeSpan.FromHours(commandTimeoutInHours));

                    await db.Log
                        .Where(t => t.Date.Date < DateTime.UtcNow.AddDays(logValidInDays * -1).Date)
                        .ExecuteDeleteAsync(cancellationToken);

                    break;
                }
                catch
                {
                    retryCount++;
                    commandTimeoutInHours *= 2;
                }
            }

            logger.LogInformation("End task.");
        }
    }
}
