using Izm.Rumis.Api.Services;
using Izm.Rumis.Infrastructure;
using Izm.Rumis.Infrastructure.Seeders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Izm.Rumis.Api.Extensions
{
    public static class IHostExtensions
    {
        public static IHost UpdateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                // Since the log table is included in the main database context (including migrations),
                // we cannot (guaranteed) write logs to the database before it's updated.
                // Use the pre-defined file logger instead.
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<InstallationLogger>>();

                try
                {
                    logger.LogInformation("Migrate the database.");

                    AppDbContextSetup.MigrateAsync(context).Wait();

                    logger.LogInformation("Seed the database.");

                    AppDbContextSetup.SeedAsync(
                        context: context,
                        options: new AppDbContextSeedOptions
                        {
                            AppUrl = Environment.GetEnvironmentVariable(EnvironmentVariable.AppUrl)
                                ?? config.GetValue<string>("Common:AppUrl")
                        },
                        identityOptions: new IdentityOptions
                        {
                            AdminPassword = Environment.GetEnvironmentVariable(EnvironmentVariable.AdminPassword)
                                ?? config.GetValue<string>("Auth:AdminPassword"),
                            ExternalAdminRole = Environment.GetEnvironmentVariable(EnvironmentVariable.ExternalAdminRole)
                                ?? config.GetValue<string>("Auth:ExternalAdminRole")
                        }).Wait();

                    logger.LogInformation("Database setup finished.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                }
            }

            return host;
        }
    }
}
