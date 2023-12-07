using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Izm.Rumis.Auth.Controllers
{
    public class HomeController : Controller
    {
        protected readonly AppSettings options;

        public HomeController(IOptions<AppSettings> options)
        {
            this.options = options.Value;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var opts = new List<string>();

            if (options.AdfsEnabled)
                opts.Add("adfs");

            if (options.WindowsEnabled)
                opts.Add("windows");

            return View(opts);
        }
    }
}
