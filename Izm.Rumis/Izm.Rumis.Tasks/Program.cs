using Izm.Rumis.Application;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Infrastructure;
using Izm.Rumis.Logging;
using Izm.Rumis.Tasks.Common;
using Izm.Rumis.Tasks.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.Reflection;

namespace Izm.Rumis.Tasks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                .Build()
                .ConfigureLogging();

            using (var scope = host.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Run the Tasks program.");
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseWindowsService(options =>
            {
                options.ServiceName = "Izm.Rumis.Tasks";
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddCustomLogging();
            })
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;

                // MediatR
                services.AddMediatR(options =>
                {
                    options.RegisterServicesFromAssemblies(Assembly.Load("Izm.Rumis.Infrastructure"));
                });

                // Redis
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = Environment.GetEnvironmentVariable(EnvironmentVariable.RedisConnectionString)
                        ?? configuration.GetConnectionString("Redis")
                        ?? throw new ConfigurationErrorsException("Redis connection string not found.");
                    options.InstanceName = "Rumis";
                });

                services.AddInfrastructure(hostContext.HostingEnvironment, options =>
                {
                    // Database
                    options.Database.ConnectionString = Environment.GetEnvironmentVariable(EnvironmentVariable.DatabaseConnectionString)
                        ?? configuration.GetConnectionString("Default");
                    options.Database.MariaDbVersion = Environment.GetEnvironmentVariable(EnvironmentVariable.MySqlVersion)
                        ?? configuration["MySql:Version"];

                    // Session
                    var idleTimeoutInMinutesString = Environment.GetEnvironmentVariable(EnvironmentVariable.SessionIdleTimeoutInMinutes)
                        ?? configuration["Auth:IdleTimeoutInMinutes"];

                    if (!int.TryParse(idleTimeoutInMinutesString, out var idleTimeoutInMinutes))
                        idleTimeoutInMinutes = 15;

                    options.Session.IdleTimeout = TimeSpan.FromMinutes(idleTimeoutInMinutes);

                    // S3
                    options.S3.StorageUrl = Environment.GetEnvironmentVariable(EnvironmentVariable.S3StorageUrl)
                        ?? configuration["S3:StorageUrl"];

                    var useHttpString = Environment.GetEnvironmentVariable(EnvironmentVariable.S3UseHttp);

                    if (!bool.TryParse(useHttpString, out var useHttp))
                        useHttp = configuration.GetValue<bool>("S3:UseHttp");

                    options.S3.UseHttp = useHttp;

                    options.S3.SecretKey = Environment.GetEnvironmentVariable(EnvironmentVariable.S3SecretKey)
                        ?? configuration["S3:SecretKey"];
                    options.S3.AccessKey = Environment.GetEnvironmentVariable(EnvironmentVariable.S3AccessKey)
                        ?? configuration["S3:AccessKey"];
                    options.Viis.EndPointAddress = Environment.GetEnvironmentVariable(EnvironmentVariable.VISSServiceEndpointAddress)
                        ?? configuration["VIIS:ServiceEndpointAddress"];
                    options.Viis.UserName = Environment.GetEnvironmentVariable(EnvironmentVariable.VIISCredentialsUsername)
                        ?? configuration["VIIS:CredentialsUsername"];
                    options.Viis.Password = Environment.GetEnvironmentVariable(EnvironmentVariable.VIISCredentialsPassword)
                        ?? configuration["VIIS:CredentialsPassword"];

                    var studentPersonCodeCacheDurationString = Environment.GetEnvironmentVariable(EnvironmentVariable.ViisStudentPersonCodeCacheDuration);

                    if (!int.TryParse(studentPersonCodeCacheDurationString, out var studentPersonCodeCacheDuration))
                        studentPersonCodeCacheDuration = configuration.GetValue<int>("VIIS:StudentPersonCodeCacheDuration");

                    options.Viis.StudentPersonCodeCacheDuration = studentPersonCodeCacheDuration;
                });
                services.AddApplication();

                services.AddSingleton<ICurrentUserService, CurrentUserService>();
                services.AddSingleton<ICurrentUserProfileService, CurrentUserProfileService>();

                services.AddTransient<HostedServiceTimer>();

                // Task services
                services.AddSingleton<ApplicationMonitoringTaskService>();
                services.AddHostedService(provider => provider.GetRequiredService<ApplicationMonitoringTaskService>());
                services.AddSingleton<SyncEducationalInstitutionsTaskService>();
                services.AddHostedService(provider => provider.GetRequiredService<SyncEducationalInstitutionsTaskService>());
                services.AddSingleton<SessionGarbageCollectionTask>();
                services.AddHostedService(provider => provider.GetRequiredService<SessionGarbageCollectionTask>());
                services.AddSingleton<ClearLogsTaskService>();
                services.AddHostedService(provider => provider.GetRequiredService<ClearLogsTaskService>());
                services.AddSingleton<ClearGdprAuditsTaskService>();
                services.AddHostedService(provider => provider.GetRequiredService<ClearGdprAuditsTaskService>());

                // Task manager
                services.AddHostedService<TaskManager>();
            });
    }
}
