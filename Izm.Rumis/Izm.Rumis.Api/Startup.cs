using HealthChecks.UI.Client;
using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Middleware;
using Izm.Rumis.Api.Options;
using Izm.Rumis.Api.Services;
using Izm.Rumis.Api.Swagger;
using Izm.Rumis.Application;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Infrastructure;
using Izm.Rumis.Infrastructure.Vraa;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Izm.Rumis.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Setting minimal thread count in ThreadPool
            int minThreadCount = 500;

            var configMinThreadCountString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.MinThreadCount)
                ?? Configuration["Common:MinThreadCount"].ToString();

            if (int.TryParse(configMinThreadCountString, out int configMinThreadCount))
                minThreadCount = configMinThreadCount;

            ThreadPool.SetMinThreads(minThreadCount, minThreadCount);

            // MediatR
            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssemblies(Assembly.Load("Izm.Rumis.Api"));
                options.RegisterServicesFromAssemblies(Assembly.Load("Izm.Rumis.Infrastructure"));
            });

            // Data protection
            //
            // This is necessary to ensure session key data protection across
            // multiple instaces of this application.
            // 
            // https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-6.0
            var redisConnectionsMultiplexer = ConnectionMultiplexer.Connect(System.Environment.GetEnvironmentVariable(EnvironmentVariable.RedisConnectionString)
                ?? Configuration.GetConnectionString("Redis"))
                ?? throw new ConfigurationErrorsException("Redis connection string not found.");

            services.AddDataProtection()
                .PersistKeysToStackExchangeRedis(redisConnectionsMultiplexer, "DataProtectionKeys");

            // Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionsMultiplexer.Configuration;
                options.InstanceName = "Rumis";
            });

            // Session
            services.AddSession(options =>
            {
                var idleTimeoutInMinutesString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.SessionIdleTimeoutInMinutes)
                    ?? Configuration["Auth:IdleTimeoutInMinutes"];

                if (!int.TryParse(idleTimeoutInMinutesString, out var idleTimeoutInMinutes))
                    idleTimeoutInMinutes = 15;

                options.IdleTimeout = TimeSpan.FromMinutes(idleTimeoutInMinutes);
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Name = SessionCookie.Name;

                if (!Environment.IsDevelopment())
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            // Settings & options
            services.Configure<AppSettings>(options =>
            {
                options.AppUrl = System.Environment.GetEnvironmentVariable(EnvironmentVariable.AppUrl)
                    ?? Configuration["Common:AppUrl"];
                options.StaticFilePath = System.Environment.GetEnvironmentVariable(EnvironmentVariable.StaticFilePath)
                    ?? Configuration["Common:StaticFilePath"];
            });
            services.Configure<AuthSettings>(options =>
            {
                var auth = Configuration.GetSection("Auth");

                options.AppUrl = System.Environment.GetEnvironmentVariable(EnvironmentVariable.AppUrl)
                    ?? Configuration["Common:AppUrl"];

                var externalEnabledString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthExternalEnabled);

                if (!bool.TryParse(externalEnabledString, out var externalEnabled))
                    externalEnabled = auth.GetValue<bool>("ExternalEnabled");

                options.ExternalEnabled = externalEnabled;

                var formsEnabledString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthFormsEnabled);

                if (!bool.TryParse(formsEnabledString, out var formsEnabled))
                    formsEnabled = auth.GetValue<bool>("FormsEnabled");

                options.FormsEnabled = formsEnabled;

                options.ExternalUrl = System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthExternalUrl)
                    ?? auth.GetValue<string>("ExternalUrl");
                options.ExternalProvider = System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthExternalProvider)
                    ?? auth.GetValue<string>("ExternalProvider");
                options.AdminUserName = System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthExternalAdminUserName)
                    ?? auth.GetValue<string>("AdminUserName");
                //options.TokenLifeTime = TimeSpan.FromMinutes(int.Parse(System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthTokenLifeTimeInMinutes)
                //    ?? Configuration.GetValue<string>("Auth:TokenLifeTimeInMinutes")));
                options.TicketPassword = System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthTicketPassword)
                    ?? auth.GetValue<string>("TicketPassword");
                options.TokenSecurityKey = System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthTokenSecurityKey)
                    ?? auth.GetValue<string>("TokenSecurityKey");

                var sessionIdleTimeoutInMinutesString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.SessionIdleTimeoutInMinutes);

                if (!int.TryParse(sessionIdleTimeoutInMinutesString, out var sessionIdleTimeoutInMinutes))
                    sessionIdleTimeoutInMinutes = auth.GetValue<int?>("Auth:IdleTimeout") ?? 15;

                options.SessionIdleTimeout = TimeSpan.FromMinutes(sessionIdleTimeoutInMinutes);

                var notifyBeforeTimeoutInMinutesString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthNotifyBeforeTimeoutInMinutes);

                if (!int.TryParse(notifyBeforeTimeoutInMinutesString, out var notfiyBeforeTimeoutInMinutes))
                    notfiyBeforeTimeoutInMinutes = auth.GetValue<int?>("NotifyBeforeTimeoutInMinutes") ?? 2;

                options.NotifyBeforeSessionTimeout = TimeSpan.FromMinutes(notfiyBeforeTimeoutInMinutes);
            });
            services.Configure<AuthUserProfileOptions>(options =>
            {
                //options.TokenLifeTime = TimeSpan.FromMinutes(int.Parse(System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthUserProfileTokenLifeTimeInMinutes)
                //    ?? Configuration.GetValue<string>("Auth:UserProfile:TokenLifeTimeInMinutes")));
                options.TokenSecurityKey = System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthUserProfileTokenSecurityKey)
                    ?? Configuration.GetValue<string>("Auth:UserProfile:TokenSecurityKey");
            });

            // Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });

                options.OperationFilter<ProfileHeaderOptionFilter>();

                options.CustomSchemaIds(x => x.FullName);
            });

            // Auth
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                        System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthTokenSecurityKey)
                        ?? Configuration["Auth:TokenSecurityKey"]
                        ?? throw new ConfigurationErrorsException("Token security key not provided.")
                        )),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero // invalidate token immediatelly when expired
                };
            }).AddScheme<VraaAuthenticationSchemeOptions, VraaAuthenticationHandler>(VraaDefaults.AuthenticationScheme, options => { });

            // Application & Infrastructure
            services.AddApplication();
            services.AddInfrastructure(Environment, options =>
            {
                // Database
                options.Database.ConnectionString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.DatabaseConnectionString)
                    ?? Configuration.GetConnectionString("Default");
                options.Database.MariaDbVersion = System.Environment.GetEnvironmentVariable(EnvironmentVariable.MySqlVersion)
                    ?? Configuration["MySql:Version"];

                // EAddress
                options.EAddress.ServiceApiUrl = System.Environment.GetEnvironmentVariable(EnvironmentVariable.EAddressServiceApiUrl)
                    ?? Configuration["EAddress:ServiceApiUrl"];

                var eAddressValidateServerCertificateString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.EAddressValidateServerCertificate)
                    ?? Configuration["EAddress:ValidateServerCertificate"];

                if (!bool.TryParse(eAddressValidateServerCertificateString, out var eAddressValidateServerCertificate))
                    eAddressValidateServerCertificate = true;

                options.EAddress.ValidateServerCertificate = eAddressValidateServerCertificate;

                // Notifications
                var notificationsEnabledString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.NotificationsEnabled)
                    ?? Configuration["Common:NotificationsEnabled"];

                if (!bool.TryParse(notificationsEnabledString, out var notificationsEnabled))
                    notificationsEnabled = true;

                options.Notification.Enabled = notificationsEnabled;

                var eAddressEnabledString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.NotificationsEAddressEnabled)
                    ?? Configuration["Common:NotificationsEAddressEnabled"];

                if (!bool.TryParse(eAddressEnabledString, out var eAddressEnabled))
                    eAddressEnabled = true;

                options.Notification.EAddressEnabled = eAddressEnabled;

                options.Notification.EServicePublicUrl = System.Environment.GetEnvironmentVariable(EnvironmentVariable.EServicePublicUrl)
                    ?? Configuration["Common:EServicePublicUrl"];

                // Redis
                options.Redis.ConnectionString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.RedisConnectionString)
                    ?? Configuration.GetConnectionString("Redis");

                // Session
                var idleTimeoutInMinutesString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.SessionIdleTimeoutInMinutes)
                    ?? Configuration["Auth:IdleTimeoutInMinutes"];

                if (!int.TryParse(idleTimeoutInMinutesString, out var idleTimeoutInMinutes))
                    idleTimeoutInMinutes = 15;

                options.Session.IdleTimeout = TimeSpan.FromMinutes(idleTimeoutInMinutes);

                // S3
                options.S3.StorageUrl = System.Environment.GetEnvironmentVariable(EnvironmentVariable.S3StorageUrl)
                    ?? Configuration["S3:StorageUrl"];

                var useHttpString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.S3UseHttp);

                if (!bool.TryParse(useHttpString, out var useHttp))
                    useHttp = Configuration.GetValue<bool>("S3:UseHttp");

                options.S3.UseHttp = useHttp;

                options.S3.SecretKey = System.Environment.GetEnvironmentVariable(EnvironmentVariable.S3SecretKey)
                    ?? Configuration["S3:SecretKey"];
                options.S3.AccessKey = System.Environment.GetEnvironmentVariable(EnvironmentVariable.S3AccessKey)
                    ?? Configuration["S3:AccessKey"];
                options.S3.BucketName = System.Environment.GetEnvironmentVariable(EnvironmentVariable.S3BucketName)
                    ?? Configuration["S3:BucketName"];

                // SMTP
                var smtpEnableSslString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.SmtpEnabledSsl);

                if (!bool.TryParse(smtpEnableSslString, out var smtpEnableSsl))
                    smtpEnableSsl = Configuration.GetValue<bool>("Smtp:EnableSsl");

                options.Smtp.EnableSsl = smtpEnableSsl;
                options.Smtp.From = System.Environment.GetEnvironmentVariable(EnvironmentVariable.SmtpFrom)
                    ?? Configuration["Smtp:From"];
                options.Smtp.Password = System.Environment.GetEnvironmentVariable(EnvironmentVariable.SmtpPassword)
                    ?? Configuration["Smtp:Password"];

                var smtpPortString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.SmtpPort);

                if (!int.TryParse(smtpPortString, out var smtpPort))
                    smtpPort = Configuration.GetValue<int>("Smtp:Port");

                options.Smtp.Port = smtpPort;
                options.Smtp.Server = System.Environment.GetEnvironmentVariable(EnvironmentVariable.SmtpServer)
                    ?? Configuration["Smtp:Server"];
                options.Smtp.Username = System.Environment.GetEnvironmentVariable(EnvironmentVariable.SmtpUsername)
                    ?? Configuration["Smtp:Username"];

                // VIIS
                options.Viis.EndPointAddress = System.Environment.GetEnvironmentVariable(EnvironmentVariable.ViisServiceEndpointAddress)
                        ?? Configuration["VIIS:ServiceEndpointAddress"];
                options.Viis.UserName = System.Environment.GetEnvironmentVariable(EnvironmentVariable.ViisCredentialsUsername)
                    ?? Configuration["VIIS:CredentialsUsername"];
                options.Viis.Password = System.Environment.GetEnvironmentVariable(EnvironmentVariable.ViisCredentialsPassword)
                    ?? Configuration["VIIS:CredentialsPassword"];

                var studentPersonCodeCacheDurationString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.ViisStudentPersonCodeCacheDuration);

                if (!int.TryParse(studentPersonCodeCacheDurationString, out var studentPersonCodeCacheDuration))
                    studentPersonCodeCacheDuration = Configuration.GetValue<int>("VIIS:StudentPersonCodeCacheDuration");

                options.Viis.StudentPersonCodeCacheDuration = studentPersonCodeCacheDuration;

                // VRAA
                options.Vraa.BaseUrl = System.Environment.GetEnvironmentVariable(EnvironmentVariable.VraaBaseUrl)
                    ?? Configuration["VRAA:BaseUrl"];
                options.Vraa.ClientId = System.Environment.GetEnvironmentVariable(EnvironmentVariable.VraaClientId)
                    ?? Configuration["VRAA:ClientId"];
                options.Vraa.ClientSecret = System.Environment.GetEnvironmentVariable(EnvironmentVariable.VraaClientSecret)
                    ?? Configuration["VRAA:ClientSecret"];
            });

            // Services
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<CurrentUserProfileService>();
            services.AddScoped<ICurrentUserProfileService, CurrentUserProfileService>();
            services.AddScoped<IPasswordManager, PasswordManager>();
            services.AddScoped<IFileManager, FileManager>();
            services.AddScoped<IEncryptionService, EncryptionService>(provider =>
            {
                var password = System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthEncryptionPassword)
                    ?? Configuration["Auth:EncryptionPassword"];
                var salt = System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthEncryptionSalt)
                    ?? Configuration["Auth:EncryptionSalt"];

                return new EncryptionService(password, salt);
            });

            services.AddHttpContextAccessor();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Api config
            services.AddAntiforgery(options =>
            {
                options.SuppressXFrameOptionsHeader = true;
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    var origins = new string[]
                    {
                        System.Environment.GetEnvironmentVariable(EnvironmentVariable.AppUrl)
                            ?? Configuration["Common:AppUrl"],
                        System.Environment.GetEnvironmentVariable(EnvironmentVariable.AuthExternalUrl)
                            ?? Configuration["Auth:ExternalUrl"]
                    };

                    builder
                        .WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // must be set to support cookies
                });
            });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

            var staticFileCacheDurationString = System.Environment.GetEnvironmentVariable(EnvironmentVariable.StaticFileCacheDuration);

            if (!int.TryParse(staticFileCacheDurationString, out var staticFileCacheDuration))
                staticFileCacheDuration = Configuration.GetSection("Common").GetValue<int>("StaticFileCacheDuration");

            StaticFileResponseCacheConfig.Duration = staticFileCacheDuration;

            // Health checks
            services.AddHealthChecks()
                .AddRedis(redisConnectionsMultiplexer)
                .AddMySql(System.Environment.GetEnvironmentVariable(EnvironmentVariable.DatabaseConnectionString)
                    ?? Configuration.GetConnectionString("Default"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //else
            //{
            //    app.UseHsts();
            //}

            if (!env.IsProduction())
            {
                app.UseSwagger(options =>
                {
                    options.RouteTemplate = "_api/swagger/{documentname}/swagger.json";
                });
                app.UseSwaggerUI(setup =>
                {
                    setup.SwaggerEndpoint("/_api/swagger/v1/swagger.json", "API v1");
                    setup.RoutePrefix = "_api/docs";
                });
            }

            app.UseHealthChecks("/healthz/ready", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.UseHealthChecks("/healthz/live", new HealthCheckOptions
            {
                Predicate = _ => false
            });

            app.UseSession();

            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();

            app.UseMiddleware<Log4NetPropertyProviderMiddleware>();

            app.UseMiddleware<SessionAuthenticationMiddleware>();

            app.UseMiddleware<SessionAuthorizationMiddleware>();

            app.UseAuthorization();

            // This applications is expected to be shipped as a container with some
            // kind of a reverse proxy in front of it, Auth and App application.
            // All of those should share the same headers. Configuration should be done on reverse proxy.
            // 
            //app.Use(async (context, next) =>
            //{
            //    context.Response.Headers.Add("X-Frame-Options", "DENY");
            //    context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
            //    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            //    context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", "none");
            //    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
            //    context.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

            //    await next();
            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                    .RequireAuthorization();
            });
        }
    }
}
