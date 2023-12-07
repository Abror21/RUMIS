using Izm.Rumis.Auth.Core;
using Izm.Rumis.Infrastructure.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Izm.Rumis.Auth.Controllers
{
    public abstract class AuthController : Controller
    {
        public abstract string Type { get; }

        protected readonly IHttpClientFactory httpFactory;
        protected readonly AppSettings options;
        protected readonly ILogger logger;
        protected readonly IWebHostEnvironment env;

        protected readonly bool isDevelopment;
        protected readonly bool isStaging;

        public AuthController(IHttpClientFactory httpFactory, IOptions<AppSettings> options, ILogger logger, IWebHostEnvironment env)
        {
            this.httpFactory = httpFactory;
            this.options = options.Value;
            this.logger = logger;
            this.env = env;

            isDevelopment = env.IsDevelopment();
            isStaging = env.IsStaging();
        }

        [AllowAnonymous]
        public virtual async Task<IActionResult> Development(string returnUrl = null)
        {
            if (isDevelopment)
            {
                return await SignIn(IdentityFactory.CreateDevIdentity(), returnUrl);
            }
            else
            {
                logger.LogWarning("This route is for Development environment only.");
            }

            return Redirect(options.ErrorRedirectUrl);
        }

        [AllowAnonymous]
        public virtual async Task<IActionResult> Staging(int user, string returnUrl = null)
        {
            if (isStaging)
            {
                return await SignIn(IdentityFactory.CreateTestIdentity(user), returnUrl);
            }
            else
            {
                logger.LogWarning("This route is for Staging environment only.");
            }

            return Redirect(options.ErrorRedirectUrl);
        }

        [AllowAnonymous]
        protected virtual async Task<IActionResult> SignIn(ClaimsIdentity user, string returnUrl = null)
        {
            logger.LogInformation("User authenticated.");

            var client = httpFactory.CreateClient();

            var claimsJson = JsonConvert.SerializeObject(user.Claims.Select(t => new
            {
                t.Type,
                t.Value
            }));

            string ticket = StringCipher.Encrypt(claimsJson, options.TicketPassword);

            logger.LogInformation("Generated an auth ticket.");

            var request = new HttpRequestMessage(HttpMethod.Post, options.TicketReplyUrl);
            var model = new AuthenticateChallengeModel
            {
                Ticket = ticket,
                Type = Type
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            logger.LogInformation("Send the ticket to the consumer.");

            try
            {
                var response = await client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                logger.LogInformation($"Consumer response: {responseString}");

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Consumer auth succeeded.");

                    var data = JsonConvert.DeserializeObject<AuthenticateChallengeResponseModel>(responseString);
                    var url = data.RedirectUrl;

                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        url += $"{(url.IndexOf("?") > 0 ? "&" : "?")}returnUrl={returnUrl}";
                    }

                    logger.LogInformation("Redirect to {url}", url);

                    return Redirect(url);
                }
                else
                {
                    logger.LogError("Consumer auth failed.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured in the consumer authentication process.");
            }

            return Redirect(options.ErrorRedirectUrl);
        }
    }
}
