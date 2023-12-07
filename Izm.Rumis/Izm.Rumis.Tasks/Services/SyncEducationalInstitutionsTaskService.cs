using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.Viis;
using Izm.Rumis.Tasks.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Tasks.Services
{
    public sealed class SyncEducationalInstitutionsTaskService : TaskService
    {
        public override string ParameterCodeEnabled => ParameterCode.EducationalInstitutionTaskEnabled;
        public override string ParameterCodeIntervalInMinutes => ParameterCode.EducationalInstitutionTaskIntervalInMinutes;
        public override string ParameterCodeStartTime => ParameterCode.EducationalInstitutionTaskStartTime;

        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<SyncEducationalInstitutionsTaskService> logger;

        public SyncEducationalInstitutionsTaskService(
            IServiceScopeFactory serviceScopeFactory, 
            ILogger<SyncEducationalInstitutionsTaskService> logger,
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

            await viisService.SyncEducationalInstitutionsAsync(cancellationToken);

            logger.LogInformation("End task.");
        }
    }
}
