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
    public sealed class ClearGdprAuditsTaskService : TaskService
    {
        public override string ParameterCodeEnabled => ParameterCode.ClearGdprAuditsTaskEnabled;
        public override string ParameterCodeIntervalInMinutes => ParameterCode.ClearGdprAuditsTaskIntervalInMinutes;
        public override string ParameterCodeStartTime => ParameterCode.ClearGdprAuditsTaskStartTime;

        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<ClearGdprAuditsTaskService> logger;

        public ClearGdprAuditsTaskService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ClearGdprAuditsTaskService> logger,
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

            var gdrpValidInDays = int.Parse((await db.Parameters.FirstAsync(t => t.Code == ParameterCode.ClearGdprAuditsTaskGdprValidInDays, cancellationToken)).Value);

            var date = DateTime.UtcNow.AddDays(gdrpValidInDays * -1).Date;

            int retryCount = 0;

            while (retryCount < 3)
            {
                try
                {
                    db.Database.SetCommandTimeout(TimeSpan.FromHours(commandTimeoutInHours));

                    await db.GdprAuditData
                        .Where(t => t.Gdpr.Created.Date < date)
                        .ExecuteDeleteAsync(cancellationToken);

                    await db.GdprAudits
                        .Where(t => t.Created.Date < date)
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
