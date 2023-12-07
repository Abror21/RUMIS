using Izm.Rumis.Auth.Core;
using Izm.Rumis.Auth.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Izm.Rumis.Auth
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWsFedAuthentication(options =>
            {
                options.BaseUrl = Environment.GetEnvironmentVariable(EnvironmentVariable.AdfsBaseUrl)
                    ?? Configuration["Adfs:BaseUrl"];
                options.Wtrealm = Environment.GetEnvironmentVariable(EnvironmentVariable.AdfsWtrealm)
                    ?? Configuration["Adfs:Wtrealm"];
                options.MetadataAddress = Environment.GetEnvironmentVariable(EnvironmentVariable.AdfsMetadataAddress)
                    ?? Configuration["Adfs:MetadataAddress"];
            });
            //services.AddWindowsAuthentication(Configuration);

            services.AddHttpContextAccessor();

            services.AddHttpClient();
            services.AddControllersWithViews();

            services.Configure<AppSettings>(options =>
            {
                if (!bool.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariable.AdfsEnabled), out var adfsEnabled))
                    adfsEnabled = Configuration.GetValue<bool>("Common:AdfsEnabled");

                options.AdfsEnabled = adfsEnabled;

                if (!bool.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariable.WindowsEnabled), out var windowsEnabled))
                    windowsEnabled = Configuration.GetValue<bool>("Common:WindowsEnabled");

                options.WindowsEnabled = windowsEnabled;

                options.ErrorRedirectUrl = Environment.GetEnvironmentVariable(EnvironmentVariable.ErrorRedirectUrl)
                    ?? Configuration["Common:ErrorRedirectUrl"];
                options.SignOutRedirectUrl = Environment.GetEnvironmentVariable(EnvironmentVariable.SignOutRedirectUrl)
                    ?? Configuration["Common:SignOutRedirectUrl"];
                options.TicketPassword = Environment.GetEnvironmentVariable(EnvironmentVariable.TicketPassword)
                    ?? Configuration["Common:TicketPassword"];
                options.TicketReplyUrl = Environment.GetEnvironmentVariable(EnvironmentVariable.TicketReplyUrl)
                    ?? Configuration["Common:TicketReplyUrl"];
            });

            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            app.UseHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => false
            });

            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            //app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "_auth/{controller=Home}/{action=Index}/{id?}"
                    ).RequireAuthorization();
            });
        }
    }
}
