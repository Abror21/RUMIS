using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Core;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Helpers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Options;
using Izm.Rumis.Application;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Izm.Rumis.Infrastructure.Identity;
using Izm.Rumis.Infrastructure.Sessions;
using Izm.Rumis.Infrastructure.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    public class AuthController : ApiController
    {
        /// <summary>
        /// Cookie containing user authentication type
        /// </summary>
        public const string AuthTypeCookieName = "AuthType";

        public const int AuthTokenCookieExpirationDays = 7;
        public const int PartialTokenCookieExpirationDays = 1;

        private readonly AuthSettings options;
        private readonly IDistributedCache distributedCache;
        private readonly IUserManager userManager;
        private readonly IGdprAuditService gdprAuditService;
        private readonly ILogger<AuthController> logger;
        private readonly IWebHostEnvironment environment;

        public AuthController(
            IDistributedCache distributedCache,
            IUserManager userManager,
            ILogger<AuthController> logger,
            IOptions<AuthSettings> options,
            IGdprAuditService gdprAuditService,
            IWebHostEnvironment environment)
        {
            this.options = options.Value;
            this.distributedCache = distributedCache;
            this.userManager = userManager;
            this.logger = logger;
            this.gdprAuditService = gdprAuditService;
            this.environment = environment;
        }

        [HttpGet("configuration")]
        [AllowAnonymous]
        public ActionResult<AuthConfigurationResponse> Configuration([FromServices] PasswordSettings passwordSettings)
        {
            return new AuthConfigurationResponse
            {
                FormsEnabled = options.FormsEnabled,
                ExternalEnabled = options.ExternalEnabled,
                ExternalUrl = options.ExternalEnabled ? $"{options.ExternalUrl}/{options.ExternalProvider}" : null,
                ExternalLogoutUrl = options.ExternalEnabled ? $"{options.ExternalUrl}/{options.ExternalProvider.Split("/").First()}/signout" : null,
                PasswordSettings = options.FormsEnabled ? passwordSettings : null,
                SessionIdleTimeoutInMinutes = (int)options.SessionIdleTimeout.TotalMinutes,
                NotifyBeforeTimeoutInMinutes = (int)options.NotifyBeforeSessionTimeout.TotalMinutes
            };
        }

        /// <summary>
        /// Receive the auth ticket and place it the temporary storage.
        /// Redirect back to the client app with a security code to proceed with the authentication.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Exception thrown when there is a problem determining the ticket type.</exception>
        /// <exception cref="NotSupportedException">Exception thrown when provided ticket type authentication is not enabled.</exception>
        [HttpPost("ticket")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthTicketSignInResponse>> Ticket(AuthTicketSignInModel model, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Received a ticket: {ticket}.", JsonConvert.SerializeObject(model));

            UserAuthType? authType = model.Type switch
            {
                "adfs" => UserAuthType.Adfs,
                _ => null
            };

            if (authType == null)
                return BadRequest($"Unable to resolve ticket type \"{model.Type}\".");

            if (authType != UserAuthType.Forms && !options.ExternalEnabled)
                throw new NotSupportedException("External authentication is not enabled.");

            string userData = StringCipher.Decrypt(model.Ticket, options.TicketPassword);

            logger.LogInformation("Decrypted ticket data: {userData}.", userData);

            var claims = JsonConvert.DeserializeObject<List<ClaimInfo>>(userData);

            var privatePersonalIdentifier = claims.Find(t => t.Type == ClaimTypesExtensions.PrivatePersonalIdentifier)?.Value;

            if (string.IsNullOrEmpty(privatePersonalIdentifier))
            {
                logger.LogError("Invalid private personal identifier - NULL or ''.");

                return BadRequest();
            }

            if (!Utility.IsPrivatePersonalIdentifierChecksumValid(privatePersonalIdentifier))
            {
                logger.LogError("Invalid private personal identifier.");

                return BadRequest();
            }

            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForTicketOperation(privatePersonalIdentifier));

            var userId = await userManager.GetUserIdAsync(privatePersonalIdentifier, cancellationToken);

            if (userId == null)
            {
                logger.LogError("User not registered.");

                return BadRequest();
            }

            if (!userManager.GetUsers()
                .Where(t => t.Id == userId
                    && !t.IsDisabled
                    && t.Logins.Any(n => n.AuthType == UserAuthType.Adfs))
                .Any())
            {
                logger.LogError("User:{id} has no active ADFS logins.", userId);

                return BadRequest();
            }

            var response = new AuthTicketSignInResponse
            {
                RedirectUrl = $"{options.AppUrl}/error"
            };

            var feature = Request.HttpContext.Features.Get<IHttpConnectionFeature>();
            var ip = feature?.LocalIpAddress?.ToString();

            var code = Guid.NewGuid().ToString("N");

            distributedCache.SetString(code, userId.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
            });

            logger.LogInformation("Place user ID in cache using code {code}, {ip}: {userId}.", code, ip, userId);

            response.RedirectUrl = $"{options.AppUrl}/auth/login?code={code}";

            return response;
        }

        /// <summary>
        /// Log in a user.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Exception thrown when user is not found or disabled or hidden (system user).</exception>
        [HttpPost("login")]
        [AnonymousOnly]
        public async Task<ActionResult<AuthUserResponse>> Login(
            AuthLoginModel model,
            [FromServices] ISessionManager sessionManager,
            [FromServices] ISessionService sessionService,
            CancellationToken cancellationToken = default)
        {
            AuthUserResponse user = null;

            if (options.ExternalEnabled && model.Code != null)
                user = await LoginByCodeAsync(model, cancellationToken);
            else if (options.FormsEnabled)
                user = await LoginByFormsAsync(model, cancellationToken);

            if (user == null || user.IsDisabled || user.IsHidden)
            {
                if (user == null || user.IsHidden)
                    logger.LogWarning("User not found.");
                else if (user.IsDisabled)
                    logger.LogWarning("User {username} is not active.", user.UserName);

                throw new Exception(Error.AuthLoginFailed);
            }

            if (HttpContext.Session.Keys.ToArray().Any(t => t == SessionKey.UserId))
                throw new InvalidOperationException(Error.SessionAlreadyActive);

            var sessionCreated = DateTime.UtcNow;

            HttpContext.Session.SetString(SessionKey.Created, sessionCreated.ToBinary().ToString());
            HttpContext.Session.SetString(SessionKey.UserId, user.Id.ToString());

            var sessionTraceTask = sessionManager.AddActivityTraceAsync(HttpContext.Session.Id, cancellationToken);

            var refreshToken = JwtManager.GenerateRefreshToken();

            SetAuthTypeCookie(user.AuthType);

            await sessionService.CreateAsync(Guid.Parse(HttpContext.Session.Id), sessionCreated, cancellationToken);

            if (user.PersonId != null)
                await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForLoginOperation(user), cancellationToken);

            await sessionTraceTask;

            return await CreateUserResponseAsync(user, cancellationToken,
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisSessionId, HttpContext.Session.Id),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisSessionCreated, sessionCreated.ToBinary().ToString())
                );
        }

        [HttpPost("logout")]
        public async Task<ActionResult<AuthLogoutResponseModel>> Logout([FromServices] ISessionService sessionService, CancellationToken cancellationToken = default)
        {
            var authTypeCookie = Request.Cookies[AuthTypeCookieName];

            Response.Cookies.Delete(AuthTypeCookieName);

            await sessionService.DeleteAsync(Guid.Parse(HttpContext.Session.Id), cancellationToken);

            HttpContext.Session.Clear();
            Response.Cookies.Delete(SessionCookie.Name);

            return Enum.TryParse(authTypeCookie, true, out UserAuthType authType)
                ? new AuthLogoutResponseModel
                {
                    RedirectUrl = authType == UserAuthType.Forms
                        ? "/"
                        : $"{options.ExternalUrl}/{options.ExternalProvider.Split("/").First()}/signout"
                }
                : new AuthLogoutResponseModel();
        }

        private async Task<ActionResult<AuthUserResponse>> CreateUserResponseAsync(AuthUserResponse user, CancellationToken cancellationToken = default, params Claim[] claims)
        {
            var tokenClaims = new List<Claim>
            {
                ClaimHelper.CreateClaim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            if (claims.Any())
                tokenClaims.AddRange(claims);

            if (!string.IsNullOrEmpty(user.UserName))
                tokenClaims.Add(ClaimHelper.CreateClaim(ClaimTypes.Name, user.UserName));

            if (user.PersonId != null)
            {
                tokenClaims.Add(ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisPersonId, user.PersonId));

                foreach (var person in user.Persons)
                    tokenClaims.Add(ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisPerson, System.Text.Json.JsonSerializer.Serialize(person)));
            }

            var token = JwtManager.GenerateAccessToken(tokenClaims, options.TokenSecurityKey, null);

            user.AccessToken = token.Token;
            user.AccessTokenExpires = token.Expires;

            return user;
        }

        private async Task<AuthUserResponse> LoginByFormsAsync(AuthLoginModel model, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Log-in using username and password.");

            var passwordCheck = await userManager.VerifyPassword(model.UserName, model.Password);

            if (passwordCheck)
                return await userManager.GetLogins().Where(t => t.UserName == model.UserName).FirstAsync(map: UserToLoginResponse(), cancellationToken: cancellationToken);
            else
                logger.LogWarning("User {username} password verification failed.", model.UserName);

            return null;
        }

        private async Task<AuthUserResponse> LoginByCodeAsync(AuthLoginModel model, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Log-in using code: {code}.", model.Code);

            var userId = Guid.Parse(distributedCache.GetString(model.Code));
            distributedCache.Remove(model.Code);

            var feature = Request.HttpContext.Features.Get<IHttpConnectionFeature>();
            var ip = feature?.LocalIpAddress?.ToString();

            logger.LogInformation("User ID retrieved using code {code}, {ip}: {userId}.", model.Code, ip, userId);

            return await userManager.GetLogins()
                .Where(t => t.UserId == userId)
                .FirstAsync(map: UserToLoginResponse(), cancellationToken: cancellationToken);
        }

        private void SetAuthTypeCookie(UserAuthType authType)
        {
            Response.Cookies.Append(AuthTypeCookieName, authType.ToString(), CreateCookieOptions(TimeSpan.FromDays(AuthTokenCookieExpirationDays)));
        }

        private CookieOptions CreateCookieOptions(TimeSpan expires)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.Add(expires),
                SameSite = SameSiteMode.Strict
            };

            if (!environment.IsDevelopment())
                options.Secure = true;

            return options;
        }

        private Expression<Func<IdentityUserLogin, AuthUserResponse>> UserToLoginResponse()
        {
            return t => new AuthUserResponse
            {
                Id = t.User.Id,
                PersonId = t.User.User.PersonTechnical == null ? null : t.User.User.PersonTechnical.Id,
                AuthType = t.AuthType,
                UserName = t.UserName,
                IsDisabled = t.User.IsDisabled,
                IsHidden = t.User.User.IsHidden,
                LoginId = t.Id,
                MustResetPassword = t.MustResetPassword,
                Persons = t.User.User.PersonTechnical == null ? null : t.User.User.PersonTechnical.Persons.Select(person => new AuthUserResponse.PersonData
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    //PrivatePersonalIdentifier = person.PrivatePersonalIdentifier
                })
            };
        }

        public class Error
        {
            public const string AuthLoginFailed = "auth.loginFailed";
            public const string IncorrectProfileId = "auth.incorrectProfileId";
            public const string SessionAlreadyActive = "auth.sessionAlreadyActive";
            public const string UserNotFound = "user.notFound";

        }

        private class ClaimInfo
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }

        public class GdprAuditHelper
        {
            public static GdprAuditTraceDto GenerateTraceForTicketOperation(string privatePersonalIdentifier)
            {
                return new GdprAuditTraceDto
                {
                    Action = "auth.ticket",
                    ActionData = null,
                    DataOwnerId = null,
                    DataOwnerPrivatePersonalIdentifier = privatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = privatePersonalIdentifier }
                    }
                };
            }

            public static GdprAuditTraceDto GenerateTraceForLoginOperation(AuthUserResponse response)
            {
                var result = new GdprAuditTraceDto
                {
                    Action = "auth.login",
                    ActionData = null,
                    DataOwnerId = response.PersonId,
                    DataOwnerPrivatePersonalIdentifier = null
                };

                var data = new List<PersonDataProperty>();

                foreach (var person in response.Persons)
                {
                    data.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                    data.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                    //data.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                }

                result.Data = data.Where(t => t.Value != null)
                    .ToArray();

                return result;
            }
        }
    }
}
