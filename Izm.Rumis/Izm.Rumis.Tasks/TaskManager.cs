using Izm.Rumis.Application.Common;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Tasks.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Tasks
{
    public sealed class TaskManager : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<TaskManager> logger;

        public TaskManager(IServiceProvider serviceProvider, ILogger<TaskManager> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Start execution.");

            var services = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.BaseType == typeof(TaskService))
                .Select(t => (TaskService)serviceProvider.GetRequiredService(t))
                .ToArray();

            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecuteAsync(services, stoppingToken);
            }

            logger.LogInformation("Stop execution.");
        }

        private async Task ExecuteAsync(TaskService[] services, CancellationToken stoppingToken)
        {
            logger.LogInformation("Execute updates.");

            try
            {
                await UpdateServicesAsync(services, stoppingToken);

                using var scope = serviceProvider.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                var intervalParameterValue = await db.Parameters
                    .Where(t => t.Code == ParameterCode.TaskManagerIntervalInMinutes)
                    .Select(t => t.Value)
                    .FirstOrDefaultAsync(stoppingToken);

                logger.LogInformation("Updates executed. Sleeping for {minutes} minutes.", intervalParameterValue);

                Thread.Sleep((int)TimeSpan.FromMinutes(int.Parse(intervalParameterValue)).TotalMilliseconds);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured.");
            }
        }

        private Task UpdateServicesAsync(IEnumerable<TaskService> services, CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task>();

            foreach (var service in services)
                tasks.Add(UpdateServiceAsync(service, cancellationToken));

            return Task.WhenAll(tasks);
        }

        private async Task UpdateServiceAsync(TaskService service, CancellationToken cancellationToken = default)
        {
            try
            {
                await service.UpdateAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update service:{type}.", service.GetType());
            }
        }
    }
}
