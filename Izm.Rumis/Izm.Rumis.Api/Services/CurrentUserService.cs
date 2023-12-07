using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Infrastructure.Common;
using Izm.Rumis.Infrastructure.Vraa;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace Izm.Rumis.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public Guid Id { get; private set; }
        public Guid? PersonId { get; private set; }
        public string UserName { get; private set; }
        public string Email { get; private set; }
        public string Language { get; private set; }
        public string RequestUrl { get; private set; }
        public string IpAddress { get; private set; }
        public IEnumerable<PersonData> Persons { get; private set; } = Array.Empty<PersonData>();

        private const string defaultLanguage = "lv";

        private readonly ILogger<CurrentUserService> logger;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger)
        {
            this.logger = logger;

            var ctx = httpContextAccessor.HttpContext;

            if (ctx == null)
                return;

            RequestUrl = ctx.Request.Path.ToString() + ctx.Request.QueryString.ToString();
            IpAddress = ctx.Connection.RemoteIpAddress.ToString();

            ctx.Request.Headers.TryGetValue("x-app-lang", out var langHeader);
            var lang = langHeader.FirstOrDefault();

            Language = (string.IsNullOrEmpty(lang) ? defaultLanguage : lang).ToLower();

            var user = ctx.User;

            if (user == null || !user.Identity.IsAuthenticated)
            {
                logger.LogInformation("User is not authenticated.");

                return;
            }

            logger.LogInformation("User authentication type: {type}.", user.Identity.AuthenticationType);

            switch (user.Identity.AuthenticationType)
            {
                case nameof(VraaAuthenticationHandler):
                    HandleVraaIdentity(user);
                    break;
                default:
                    HandleDefaultIdentity(user);
                    break;
            }
        }

        private void HandleDefaultIdentity(ClaimsPrincipal user)
        {
            logger.LogInformation("Handle default identity.");

            Id = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));

            if (UserIds.IsSystemId(Id))
                throw new Exception("auth.invalidToken");

            PersonId = Guid.TryParse(user.FindFirstValue(ClaimTypesExtensions.RumisPersonId), out var personId)
                ? personId
                : null;
            UserName = user.FindFirstValue(ClaimTypes.Name);
            Email = user.FindFirstValue(ClaimTypes.Email);
            Persons = user.FindAll(ClaimTypesExtensions.RumisPerson)
                .Select(t => JsonSerializer.Deserialize<PersonData>(t.Value))
                .ToArray();
        }

        private void HandleVraaIdentity(ClaimsPrincipal user)
        {
            logger.LogInformation("Handle VRAA identity.");

            Id = UserIds.EServiceUser;
        }
    }
}
