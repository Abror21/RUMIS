using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Core;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Options;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Izm.Rumis.Api.Services
{
    public class CurrentUserProfileService : ICurrentUserProfileService
    {
        public const string HeaderName = "Profile";

        public Guid Id { get; set; }
        public bool IsInitialized { get; set; }
        public Guid UserId { get; set; }
        public UserProfileType Type { get; set; }
        public string Role { get; set; }
        public int? EducationalInstitutionId { get; set; }
        public int? SupervisorId { get; set; }
        public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
        public DateTime IssuedAt => jwtSecurityToken.IssuedAt;

        private readonly JwtSecurityToken jwtSecurityToken;

        public CurrentUserProfileService(IOptions<AuthUserProfileOptions> options, IHttpContextAccessor httpContextAccessor)
        {
            if (!httpContextAccessor.HttpContext.User.Identity.IsAuthenticated
                || !httpContextAccessor.HttpContext.Request.Headers.TryGetValue(HeaderName, out StringValues values))
                return;

            var token = values.FirstOrDefault();

            if (string.IsNullOrEmpty(token))
                return;

            var claimsPrincipal = GetClaimsPrincipal(token, options.Value);

            if (httpContextAccessor.HttpContext.Session.Id != claimsPrincipal.FindFirstValue(ClaimTypesExtensions.RumisSessionId)
                || httpContextAccessor.HttpContext.Session.GetString(SessionKey.Created) != claimsPrincipal.FindFirstValue(ClaimTypesExtensions.RumisSessionCreated))
                throw new UnauthorizedAccessException(Error.InvalidTokenProvided);

            UserId = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));

            var identityId = Guid.Parse(httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (identityId != UserId)
                throw new InvalidUserProfileException(identityId, UserId, "User ID mismatch.");

            Permissions = claimsPrincipal.FindAll(ClaimTypesExtensions.RumisUserProfileRolePermission).Select(t => t.Value).ToArray();
            Role = claimsPrincipal.FindFirstValue(ClaimTypesExtensions.RumisUserProfileRole);
            Type = Enum.Parse<UserProfileType>(claimsPrincipal.FindFirstValue(ClaimTypesExtensions.RumisUserProfileType));
            Id = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypesExtensions.RumisUserProfileIdentifier));

            var educationalInstitutionIdClaim = claimsPrincipal.FindFirstValue(ClaimTypesExtensions.RumisUserProfileEducationalInstitutionIdentifier);
            if (educationalInstitutionIdClaim != null)
                EducationalInstitutionId = int.Parse(educationalInstitutionIdClaim);

            var supervisorIdClaim = claimsPrincipal.FindFirstValue(ClaimTypesExtensions.RumisUserProfileSupervisorIdentifier);
            if (supervisorIdClaim != null)
                SupervisorId = int.Parse(supervisorIdClaim);

            jwtSecurityToken = new JwtSecurityToken(token);

            IsInitialized = true;
        }

        public bool HasPermission(string permission)
        {
            return Permissions.Contains(permission);
        }

        private ClaimsPrincipal GetClaimsPrincipal(string token, AuthUserProfileOptions options)
        {
            try
            {
                return JwtManager.DecodeAccessToken(token, options.TokenSecurityKey);
            }
            catch (Exception)
            {
                throw new UnauthorizedAccessException(Error.IncorrectTokenProvided);
            }
        }

        public static class Error
        {
            public const string IncorrectTokenProvided = "currentUserProfile.incorrectTokenProvided";
            public const string InvalidTokenProvided = "currentUserProfile.invalidTokenProvided";
        }
    }
}
