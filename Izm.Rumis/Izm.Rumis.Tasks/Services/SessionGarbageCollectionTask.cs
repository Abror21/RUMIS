using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.Sessions;
using Izm.Rumis.Tasks.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Tasks.Services
{
    public sealed class SessionGarbageCollectionTask : TaskService
    {
        public override string ParameterCodeEnabled => ParameterCode.SessionGarbageCollectionTaskEnabled;
        public override string ParameterCodeIntervalInMinutes => ParameterCode.SessionGarbageCollectionTaskIntervalInMinutes;
        public override string ParameterCodeStartTime => ParameterCode.SessionGarbageCollectionTaskStartTime;

        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<SessionGarbageCollectionTask> logger;

        public SessionGarbageCollectionTask(
            IServiceScopeFactory serviceScopeFactory, 
            ILogger<SessionGarbageCollectionTask> logger,
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

            // Dependencies
            var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
            var sessionManager = scope.ServiceProvider.GetRequiredService<ISessionManager>();

            // Work
            var referenceDate = DateTime.UtcNow.Add(-sessionManager.SessionIdleTimeout);

            var sessionIds = await sessionService.Get()
                .Where(t => t.Created <= referenceDate)
                .ListAsync(map: t => t.Id, cancellationToken: cancellationToken);

            var sessionsToDelete = new List<Guid>();

            foreach (var sessionId in sessionIds)
            {
                var trace = await sessionManager.GetActivityTraceAsync(sessionId.ToString(), cancellationToken);

                if (trace == null)
                    sessionsToDelete.Add(sessionId);
            }

            await sessionService.DeleteRangeAsync(sessionsToDelete, cancellationToken);

            logger.LogInformation("End task.");
        }
    }
}
