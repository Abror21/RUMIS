using Amazon.S3;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Infrastructure.EAddress;
using Izm.Rumis.Infrastructure.Email;
using Izm.Rumis.Infrastructure.Identity;
using Izm.Rumis.Infrastructure.Logging;
using Izm.Rumis.Infrastructure.Modif;
using Izm.Rumis.Infrastructure.Notifications;
using Izm.Rumis.Infrastructure.ResourceImport;
using Izm.Rumis.Infrastructure.Services;
using Izm.Rumis.Infrastructure.Sessions;
using Izm.Rumis.Infrastructure.Viis;
using Izm.Rumis.Infrastructure.Vraa;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Security;
using VIIS;

namespace Izm.Rumis.Infrastructure
{
    public sealed class InfrastructureOptions
    {
        public DatabaseOptions Database { get; set; } = new DatabaseOptions();
        public EAddressOptions EAddress { get; set; } = new EAddressOptions();
        public NotitficationOptions Notification { get; set; } = new NotitficationOptions();
        public PasswordOptions Password { get; set; } = new PasswordOptions();
        public RedisOptions Redis { get; set; } = new RedisOptions();
        public SessionOptions Session { get; set; } = new SessionOptions();
        public SmtpOptions Smtp { get; set; } = new SmtpOptions();
        public S3Options S3 { get; set; } = new S3Options();
        public ViisOptions Viis { get; set; } = new ViisOptions();
        public VraaOptions Vraa { get; set; } = new VraaOptions();

        public sealed class DatabaseOptions
        {
            public string ConnectionString { get; set; }
            public string MariaDbVersion { get; set; }
        }

        public sealed class EAddressOptions
        {
            public string ServiceApiUrl { get; set; }
            public bool ValidateServerCertificate { get; set; } = true;
        }

        public sealed class NotitficationOptions
        {
            public bool EAddressEnabled { get; set; } = true;
            public bool Enabled { get; set; } = true;
            public string EServicePublicUrl { get; set; }
        }

        public sealed class PasswordOptions
        {
            public int MaxLength { get; set; } = 32;
            public int MinLength { get; set; } = 8;
            public bool RequireDigit { get; set; } = false;
            public bool RequireLower { get; set; } = false;
            public bool RequireSpecial { get; set; } = false;
            public bool RequireUpper { get; set; } = false;
            public string SpecialChars { get; set; } = string.Empty;
        }

        public sealed class RedisOptions
        {
            public string ConnectionString { get; set; }
        }

        public sealed class SessionOptions
        {
            public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes(15);
        }

        public sealed class SmtpOptions
        {
            public bool EnableSsl { get; set; } = true;
            public string From { get; set; }
            public string Password { get; set; }
            public int Port { get; set; }
            public string Server { get; set; }
            public string Username { get; set; }
        }

        public sealed class S3Options
        {
            public string StorageUrl { get; set; }
            public bool UseHttp { get; set; }
            public string SecretKey { get; set; }
            public string AccessKey { get; set; }
            public string BucketName { get; set; }
        }

        public sealed class ViisOptions
        {
            public string EndPointAddress { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public int StudentPersonCodeCacheDuration { get; set; }
        }

        public sealed class VraaOptions
        {
            public string BaseUrl { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }
    }

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IHostEnvironment environment, Action<InfrastructureOptions> optionsBuilder = null)
        {
            var options = new InfrastructureOptions();
            optionsBuilder?.Invoke(options);

            services.AddDatabase(options.Database);
            services.AddEAddress(options.EAddress);
            services.AddRedis(options.Redis);
            services.AddEmail(options.Smtp);
            services.AddFileSystem(options.S3);
            services.AddIdentity(options.Password);
            services.AddSessions(options.Session);
            services.AddViis(options.Viis, environment);
            services.AddVraa(options.Vraa);

            services.AddScoped(provider =>
            {
                return new NotificationOptions
                {
                    EAddressEnabled = options.Notification.EAddressEnabled,
                    Enabled = options.Notification.Enabled,
                    EServicePublicUrl = options.Notification.EServicePublicUrl
                };
            });

            services.AddScoped<IEServicesService, EServicesService>();
            services.AddScoped<IResourceImportService, ResourcesImportService>();
            services.AddScoped<ILogService, LogService>();

            return services;
        }

        private static IServiceCollection AddDatabase(this IServiceCollection services, InfrastructureOptions.DatabaseOptions options)
        {
            services.AddDbContext<AppDbContext>(dbContextOptions =>
            {
                dbContextOptions.UseMySql(
                    options.ConnectionString ?? throw new ConfigurationErrorsException("Database connection string not found."),
                    new MariaDbServerVersion(Version.Parse(
                        options.MariaDbVersion ?? throw new ConfigurationErrorsException("MariaDb version not configured.")
                        ))
                );
            });

            services.AddScoped<IAppDbContext>(provider => provider.GetService<AppDbContext>());
            services.AddScoped<IIdentityDbContext>(provider => provider.GetService<AppDbContext>());
            services.AddScoped<ILogDbContext>(provider => provider.GetService<AppDbContext>());
            services.AddScoped<IModifDbContext>(provider => provider.GetService<AppDbContext>());

            services.AddScoped<IModifService, ModifService>();

            return services;
        }

        private static IServiceCollection AddEAddress(this IServiceCollection services, InfrastructureOptions.EAddressOptions options)
        {
            services.AddHttpClient<IEAddressClient, EAddressClient>(http =>
            {
                http.BaseAddress = new Uri($"{options.ServiceApiUrl?.TrimEnd('/')}/");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

                if (!options.ValidateServerCertificate)
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                return handler;
            });

            return services;
        }

        private static IServiceCollection AddEmail(this IServiceCollection services, InfrastructureOptions.SmtpOptions options)
        {
            services.Configure<EmailServiceOptions>(serviceOptions =>
            {
                serviceOptions.EnableSsl = options.EnableSsl;
                serviceOptions.From = options.From;
                serviceOptions.Password = options.Password;
                serviceOptions.Port = options.Port;
                serviceOptions.Server = options.Server;
                serviceOptions.Username = options.Username;
            });

            services.AddScoped<IEmailService, EmailService>();

            return services;
        }

        private static IServiceCollection AddFileSystem(this IServiceCollection services, InfrastructureOptions.S3Options options)
        {
            services.AddSingleton<IAmazonS3>(provider => new AmazonS3Client(
                options.AccessKey ?? throw new ConfigurationErrorsException("File storage access key not found."),
                options.SecretKey ?? throw new ConfigurationErrorsException("File storage secret key not found."),
                new AmazonS3Config
                {
                    ServiceURL = options.StorageUrl ?? throw new ConfigurationErrorsException("File storage url not found."),
                    UseHttp = options.UseHttp,
                    ForcePathStyle = true
                }));


            services.AddSingleton(provider =>
            {
                return new FileServiceOptions
                {
                    S3BucketName = options.BucketName ?? throw new ConfigurationErrorsException("File storage bucket name not found.")
                    // Uncomment if this app works with Access database files
                    //AccessDbProvider = configuration.GetValue<string>("AppSettings:AccessDbProvider")
                };
            });

            services.AddScoped<IFileService, FileService>();

            return services;
        }

        private static IServiceCollection AddIdentity(this IServiceCollection services, InfrastructureOptions.PasswordOptions options)
        {
            services.AddSingleton(provider =>
            {
                var settings = new PasswordSettings
                {
                    MaxLength = options.MaxLength,
                    MinLength = options.MinLength,
                    RequireDigit = options.RequireDigit,
                    RequireLower = options.RequireLower,
                    RequireSpecial = options.RequireSpecial,
                    RequireUpper = options.RequireUpper,
                    SpecialChars = options.SpecialChars
                };

                if (string.IsNullOrEmpty(settings.SpecialChars))
                    settings.SpecialChars = PasswordSettings.DefaultSpecialChars;

                return settings;
            });

            services.AddScoped<IPasswordValidator, PasswordValidator>();
            services.AddScoped<IUserManager, UserManager>();

            return services;
        }

        private static IServiceCollection AddRedis(this IServiceCollection services, InfrastructureOptions.RedisOptions options)
        {
            services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(options.ConnectionString));
            services.AddScoped<IDatabase>(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

            services.AddScoped<ISequenceService, RedisService>();

            return services;
        }

        private static IServiceCollection AddSessions(this IServiceCollection services, InfrastructureOptions.SessionOptions options)
        {
            services.Configure<SessionManagerOptions>(sessionManagerOptions =>
            {
                sessionManagerOptions.SessionIdleTimeout = options.IdleTimeout;
            });

            services.AddSingleton<ISessionManager, SessionManager>();
            services.AddScoped<ISessionDbContext, AppDbContext>();
            services.AddScoped<ISessionService, SessionService>();

            return services;
        }

        private static IServiceCollection AddViis(this IServiceCollection services, InfrastructureOptions.ViisOptions options, IHostEnvironment environment)
        {
            services.Configure<ViisServiceOptions>(vissOptions =>
            {
                vissOptions.StudentPersonCodeCacheDuration = options.StudentPersonCodeCacheDuration;
            });

            services.AddScoped<IViisService, ViisService>();
            services.AddScoped<IApplicationSocialStatusCheckService, ViisService>();
            services.AddScoped<IPersonDataService, ViisService>();
            services.AddScoped<IUniversalDataSetService>(provider =>
            {
                var client = new UniversalDataSetServiceClient();
                client.ClientCredentials.UserName.UserName = options.UserName;
                client.ClientCredentials.UserName.Password = options.Password;

                if (!environment.IsProduction())
                {
                    client.ClientCredentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck
                    };
                }

                var endpoint = new EndpointAddress(options.EndPointAddress);
                client.Endpoint.Address = endpoint;

                return client;
            });

            return services;
        }

        private static IServiceCollection AddVraa(this IServiceCollection services, InfrastructureOptions.VraaOptions options)
        {
            services.AddHttpContextAccessor();

            services.AddHttpClient<IVraaClient, VraaClient>(http =>
            {
                http.BaseAddress = new Uri(options.BaseUrl);

                http.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(
                    userName: options.ClientId,
                    password: options.ClientSecret
                    );
            });

            services.AddScoped<IVraaUser, VraaUser>();

            return services;
        }
    }
}
