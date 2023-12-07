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
    public partial class WindowsController : AuthController
    {
        public override string Type => "windows";

        public WindowsController(IHttpClientFactory httpFactory, IOptions<AppSettings> options, ILogger<WindowsController> logger, IWebHostEnvironment env)
            : base(httpFactory, options, logger, env)
        {
            if (!this.options.WindowsEnabled)
                throw new NotSupportedException();
        }

        public async Task<IActionResult> Index(string returnUrl = null)
        {
            var result = await SignIn((ClaimsIdentity)User.Identity, returnUrl);
            return result;
        }

        [AllowAnonymous]
        public virtual IActionResult SignOut()
        {
            // you cannot really sign out of Windows, so just redirect to keep the flow consistent
            return Redirect(options.SignOutRedirectUrl);
        }
    }
}
