using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.Viis;
using Izm.Rumis.Tasks.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Tasks.Services
{
    public sealed class ApplicationMonitoringTaskService : TaskService
    {
        public override string ParameterCodeEnabled => ParameterCode.ApplicationMonitoringTaskEnabled;
        public override string ParameterCodeIntervalInMinutes => ParameterCode.ApplicationMonitoringTaskIntervalInMinutes;
        public override string ParameterCodeStartTime => ParameterCode.ApplicationMonitoringTaskStartTime;

        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<ApplicationMonitoringTaskService> logger;

        public ApplicationMonitoringTaskService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ApplicationMonitoringTaskService> logger,
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

            var viisService = scope.ServiceProvider.GetRequiredService<IViisService>();

            await viisService.CheckPersonApplicationsAsync(cancellationToken);

            logger.LogInformation("End task.");
        }
    }
}
