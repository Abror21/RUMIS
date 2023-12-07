using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Helpers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Options;
using Izm.Rumis.Domain.Events.UserProfile;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.EventHandlers.UserProfile.UserProfileAccessLevelChanged
{
    public sealed class KillUserProfileSessionEventHandler : INotificationHandler<UserProfileAccessLevelChangedEvent>
    {
        private readonly AuthSettings authSettings;
        private readonly IDistributedCache distributedCache;

        public KillUserProfileSessionEventHandler(IOptions<AuthSettings> authSettings, IDistributedCache distributedCache)
        {
            this.authSettings = authSettings.Value;
            this.distributedCache = distributedCache;
        }

        public Task Handle(UserProfileAccessLevelChangedEvent notification, CancellationToken cancellationToken)
        {
            var data = new UserProfileSessionKillData
            {
                Event = nameof(UserProfileAccessLevelChangedEvent),
                DateTime = DateTime.UtcNow
            };

            return distributedCache.SetAsync(
                key: SessionKeyHelper.GetKillUserProfileSessionKey(notification.UserProfileId),
                value: data,
                options: new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.Add(authSettings.SessionIdleTimeout)
                },
                token: cancellationToken
                );
        }
    }
}
