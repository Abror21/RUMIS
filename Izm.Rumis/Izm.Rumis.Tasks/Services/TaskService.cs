using Izm.Rumis.Application.Common;
using Izm.Rumis.Tasks.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Tasks.Services
{
    public abstract class TaskService : IHostedService
    {
        public bool Enabled { get; set; }
        public TimeSpan Interval { get; set; }
        public TimeSpan StartTime { get; set; }

        public abstract string ParameterCodeEnabled { get; }
        public abstract string ParameterCodeIntervalInMinutes { get; }
        public abstract string ParameterCodeStartTime { get; }

        private bool isInitialized = false;
        private IEnumerable<string> parameterCodes => new string[] { ParameterCodeEnabled, ParameterCodeIntervalInMinutes, ParameterCodeStartTime };

        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger logger;
        private readonly HostedServiceTimer timer;

        public TaskService(IServiceScopeFactory serviceScopeFactory, ILogger logger, HostedServiceTimer timer)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
            this.timer = timer;
        }

        public void ResetTimer()
        {
            if (Enabled)
                timer.Change(StartTime, Interval);
            else
                timer.Disable();
        }

        public abstract Task ExecuteAsync(CancellationToken cancellationToken = default);

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await IntializeTimerAsync(cancellationToken);

#if DEBUG
                await RunAsync(cancellationToken);
#endif

                logger.LogInformation("Start task.");

                timer.Start(async (state) => await RunAsync(cancellationToken), StartTime, Interval);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to start task.");

                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            timer.Disable();

            return Task.CompletedTask;
        }

        public async Task UpdateAsync(CancellationToken cancellationToken = default)
        {
            if (!isInitialized)
                return;

            logger.LogInformation("Update task.");

            using var scope = serviceScopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var parameters = await db.Parameters
                .Where(t => parameterCodes.Contains(t.Code))
                .Select(t => new
                {
                    t.Code,
                    t.Value
                })
                .ToArrayAsync(cancellationToken);

            var enabled = bool.Parse(parameters.First(t => t.Code == ParameterCodeEnabled).Value);
            var interval = TimeSpan.FromMinutes(
                int.Parse(parameters.First(t => t.Code == ParameterCodeIntervalInMinutes).Value)
                );
            var startTime = TimeSpan.Parse(
                parameters.First(t => t.Code == ParameterCodeStartTime).Value
                );

            if (Enabled == enabled && Interval == interval && StartTime == startTime)
            {
                logger.LogInformation("No changes in task parameters.");

                return;
            }

            Enabled = enabled;
            Interval = interval;
            StartTime = startTime;

            ResetTimer();

            logger.LogInformation("Task updated. Enabled:{enabled} | Interval:{interval} | StartTime:{StartTime}.", Enabled, Interval, StartTime);
        }

        private async Task IntializeTimerAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Initialize task timer.");

            using var scope = serviceScopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var parameters = await db.Parameters
                .Where(t => parameterCodes.Contains(t.Code))
                .Select(t => new
                {
                    t.Code,
                    t.Value
                })
                .ToArrayAsync(cancellationToken);

            var enabled = bool.Parse(parameters.First(t => t.Code == ParameterCodeEnabled).Value);
            var interval = TimeSpan.FromMinutes(
                int.Parse(parameters.First(t => t.Code == ParameterCodeIntervalInMinutes).Value)
                );
            var startTime = TimeSpan.Parse(
                parameters.First(t => t.Code == ParameterCodeStartTime).Value
                );

            Enabled = enabled;
            Interval = interval;
            StartTime = startTime;

            isInitialized = true;

            logger.LogInformation("Task timer initialzied. Enabled:{enabled} | Interval:{interval} | StartTime:{StartTime}.", Enabled, Interval, StartTime);
        }

        private async Task RunAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Run task.");

            try
            {
                await ExecuteAsync(cancellationToken);

                logger.LogInformation("Task completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Task failed.");
            }
        }
    }
}
