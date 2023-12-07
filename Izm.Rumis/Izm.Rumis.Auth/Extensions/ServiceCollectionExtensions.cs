using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Izm.Rumis.Auth.Extensions
{
    internal class WsFedAuthenticationOptions
    {
        public string BaseUrl { get; set; }
        public string Wtrealm { get; set; }
        public string MetadataAddress { get; set; }
    }

    internal static class ServiceCollectionExtensions
    {
        public static void AddWsFedAuthentication(this IServiceCollection services, Action<WsFedAuthenticationOptions> optionsBuilder)
        {
            var options = new WsFedAuthenticationOptions();
            optionsBuilder.Invoke(options);

            services
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
                })
                .AddWsFederation(wsFedOptions =>
                {
                    wsFedOptions.Wtrealm = options.Wtrealm;
                    // Ensure the same base address.
                    // When nginx proxies are used, omitting Wreply may result in incorrect scheme or port.
                    // For example, if docker is configured to listen to port 44381 (suppose host:443 is already in use),
                    // and nginx reverse proxy is configured to map {hostname}:443 back to {hostname}:44381,
                    // then wreply will be set to {hostname}:44381 and fail to redirect to the defined URL (/adfs/SignedIn)
                    // after successfull WS-FED authentication.
                    wsFedOptions.Wreply = $"{options.BaseUrl}/signin-wsfed";
                    // Optionally, define a callback path if 404 received due to web server configuration.
                    wsFedOptions.CallbackPath = "/_auth/signin-wsfed";
                    wsFedOptions.SignOutWreply = $"{options.BaseUrl}/adfs/SignedOut";
                    wsFedOptions.MetadataAddress = options.MetadataAddress;

                    wsFedOptions.CorrelationCookie.SameSite = SameSiteMode.None;
                    wsFedOptions.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                })
                .AddCookie(cookieOptions =>
                {
                    cookieOptions.Cookie.HttpOnly = true;
                    cookieOptions.ExpireTimeSpan = TimeSpan.FromSeconds(1);
                    cookieOptions.SlidingExpiration = false;
                    cookieOptions.Events.OnSigningIn = context =>
                    {
                        context.CookieOptions.Expires = DateTime.Now.AddSeconds(1);
                        return Task.CompletedTask;
                    };
                });
        }

        public static void AddWindowsAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();
        }
    }
}
