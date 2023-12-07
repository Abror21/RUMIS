using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Helpers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Services;
using Izm.Rumis.Domain.Events.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Middleware
{
    /// <summary>
    /// Authorize user based on session.
    /// </summary>
    public class SessionAuthorizationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IDistributedCache distributedCache;

        public SessionAuthorizationMiddleware(RequestDelegate next, IDistributedCache distributedCache)
        {
            this.next = next;
            this.distributedCache = distributedCache;
        }

        public async Task Invoke(HttpContext context)
        {
            var allowAnnoymous = context.GetEndpoint()?.Metadata?
                .GetMetadata<AllowAnonymousAttribute>();

            var currentUserProfile = context.RequestServices.GetRequiredService<CurrentUserProfileService>();

            if (currentUserProfile.IsInitialized)
            {
                var userProfileKillData = await distributedCache.GetFromJsonAsync<UserProfileSessionKillData>(
                    key: SessionKeyHelper.GetKillUserProfileSessionKey(currentUserProfile.Id)
                    );

                if (userProfileKillData != null
                    && userProfileKillData.DateTime >= currentUserProfile.IssuedAt.ToUniversalTime())
                {
                    var message = userProfileKillData.Event switch
                    {
                        nameof(UserProfileAccessLevelChangedEvent) => Error.AccessLevelChanged,
                        nameof(UserProfileDisabledEvent) => Error.ProfileDisabled,
                        nameof(UserProfileExpirationChangedEvent) => Error.ExpirationChanged,
                        nameof(UserProfileRolesChangedEvent) => Error.ProfileRolesChanged,
                        _ => null
                    };

                    throw new UnauthorizedAccessException(message);
                }
            }

            await next(context);
        }

        private static DateTime GetCreatedValueFromSession(HttpContext context, Guid userProfileId)
        {
            var binary = context.Session.GetString(SessionKeyHelper.GetUserProfileTokenCreatedSessionKey(userProfileId));

            return DateTime.FromBinary(long.Parse(binary));
        }

        public static class Error
        {
            public const string AccessLevelChanged = "session.accessLevelChanged";
            public const string ExpirationChanged = "session.expirationChanged";
            public const string ProfileDisabled = "session.profileDisabled";
            public const string ProfileRolesChanged = "session.profileRolesChanged";
        }
    }
}
