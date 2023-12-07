using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Izm.Rumis.Auth.Controllers
{
    public partial class AdfsController : AuthController
    {
        public override string Type => "adfs";

        public AdfsController(IHttpClientFactory httpFactory, IOptions<AppSettings> options, ILogger<AdfsController> logger, IWebHostEnvironment env)
            : base(httpFactory, options, logger, env)
        {
            if (!this.options.AdfsEnabled)
                throw new NotSupportedException();
        }

        [AllowAnonymous]
        public IActionResult Index(string returnUrl = null)
        {
            var props = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                RedirectUri = Url.Action("SignedIn", new { returnUrl })
            };

            return Challenge(props, WsFederationDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        public async Task<IActionResult> SignedIn(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return await SignIn((ClaimsIdentity)User.Identity, returnUrl);
            }
            else
            {
                logger.LogError("User is not authenticated.");
            }

            return Redirect(options.ErrorRedirectUrl);
        }

        [AllowAnonymous]
        public new IActionResult SignOut()
        {
            if (isDevelopment || isStaging)
            {
                return Redirect(Url.Action("SignedOut"));
            }

            var props = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                // WsFederation SignOutWreply is used instead
                //RedirectUri = Url.Action("SignedOut")
            };

            return SignOut(props,
                CookieAuthenticationDefaults.AuthenticationScheme,
                WsFederationDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        public virtual IActionResult SignedOut()
        {
            return Redirect(options.SignOutRedirectUrl);
        }
    }
}
