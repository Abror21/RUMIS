using Izm.Rumis.Api.Helpers;
using Izm.Rumis.Domain.Events.UserProfile;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.EventHandlers.UserProfile.UserProfileDisabled
{
    public sealed class RemoveBlacklistedUserProfileSessionEventHandler : INotificationHandler<UserProfileEnabledEvent>
    {
        private readonly IDistributedCache distributedCache;

        public RemoveBlacklistedUserProfileSessionEventHandler(IDistributedCache distributedCache)
        {
            this.distributedCache = distributedCache;
        }

        public Task Handle(UserProfileEnabledEvent notification, CancellationToken cancellationToken)
        {
            return distributedCache.RemoveAsync(
                key: SessionKeyHelper.GetKillUserProfileSessionKey(notification.UserProfileId),
                token: cancellationToken
                );
        }
    }
}
