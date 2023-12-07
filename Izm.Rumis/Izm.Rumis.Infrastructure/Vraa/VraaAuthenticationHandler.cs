using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Izm.Rumis.Infrastructure.Vraa.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Vraa
{
    public sealed class VraaAuthenticationHandler : AuthenticationHandler<VraaAuthenticationSchemeOptions>
    {
        private readonly IVraaClient vraaClient;
        private readonly IGdprAuditService gdprAuditService;

        public VraaAuthenticationHandler(
            IOptionsMonitor<VraaAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IVraaClient vraaClient,
            IGdprAuditService gdprAuditService) : base(options, logger, encoder, clock)
        {
            this.vraaClient = vraaClient;
            this.gdprAuditService = gdprAuditService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
                return AuthenticateResult.Fail(Error.HeaderNotFound);

            var header = Request.Headers[HeaderNames.Authorization].ToString();

            var token = header.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault();

            if (string.IsNullOrEmpty(token))
                return AuthenticateResult.Fail(Error.TokenNotFound);

            var introspectResult = await vraaClient.IntrospectAsync(token);

            if (!bool.Parse(introspectResult.Active))
                return AuthenticateResult.Fail(Error.TokenNotActive);

            if (string.IsNullOrEmpty(introspectResult.PrivatePersonalIdentifier))
                return AuthenticateResult.Fail(Error.TokenDoesNotIdentifyPerson);

            var claims = new[]
            {
                new Claim(ClaimTypes.GivenName, introspectResult.FirstName),
                new Claim(ClaimTypes.Surname, introspectResult.LastName),
                new Claim(ClaimTypesExtensions.PrivatePersonalIdentifier, introspectResult.PrivatePersonalIdentifier)
            };

            await gdprAuditService.TraceAsync(GdprAudtiHelper.GenerateTraceForIntrospectResult(introspectResult));

            var claimsIdentity = new ClaimsIdentity(claims, nameof(VraaAuthenticationHandler));

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        public static class Error
        {
            public const string HeaderNotFound = "vraaAuth.headerNotFound";
            public const string TokenDoesNotIdentifyPerson = "vraaAuth.tokenDoesNotIdentifyPerson";
            public const string TokenNotActive = "vraaAuth.tokenNotActive";
            public const string TokenNotFound = "vraaAuth.tokenNotFound";
        }

        public static class GdprAudtiHelper
        {
            public static GdprAuditTraceDto GenerateTraceForIntrospectResult(IntrospectResult result)
            {
                return new GdprAuditTraceDto
                {
                    Action = "vraa.introspectToken",
                    DataOwnerPrivatePersonalIdentifier = result.PrivatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.FirstName, Value = result.FirstName },
                        new PersonDataProperty { Type = PersonDataType.LastName, Value = result.LastName },
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = result.PrivatePersonalIdentifier }
                    }
                };
            }
        }
    }
}
