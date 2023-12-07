using Izm.Rumis.Api.EventHandlers.UserProfile.UserProfileDisabled;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Events.UserProfile;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.EventHandlers.UserProfile.UserProfileEnabled
{
    public sealed class RemoveBlacklistedUserProfileSessionEventHandlerTests
    {
        private static readonly UserProfileEnabledEvent notification = new UserProfileEnabledEvent(Guid.NewGuid());

        [Fact]
        public async Task Handle_Remove()
        {
            // Assign
            var distributedCache = ServiceFactory.CreateDistributedCache();

            var handler = GetHander(
                distributedCache: distributedCache
                );

            // Act
            await handler.Handle(notification, CancellationToken.None);

            // Assert
            Assert.NotNull(distributedCache.RemoveCalledWith);
        }

        private RemoveBlacklistedUserProfileSessionEventHandler GetHander(IDistributedCache distributedCache = null)
        {
            return new RemoveBlacklistedUserProfileSessionEventHandler(
                distributedCache: distributedCache ?? ServiceFactory.CreateDistributedCache()
                );
        }
    }
}
