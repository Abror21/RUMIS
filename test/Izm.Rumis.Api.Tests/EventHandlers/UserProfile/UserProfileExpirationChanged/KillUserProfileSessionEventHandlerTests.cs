using Izm.Rumis.Api.EventHandlers.UserProfile.UserProfileExpirationChanged;
using Izm.Rumis.Api.Options;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Events.UserProfile;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.EventHandlers.UserProfile.UserProfileExpirationChanged
{
    public sealed class KillUserProfileSessionEventHandlerTests
    {
        private readonly UserProfileExpirationChangedEvent notification = new UserProfileExpirationChangedEvent(Guid.NewGuid(), DateTime.Now);

        [Fact]
        public async Task Handle_AddsKillProfileCacheEntry()
        {
            // Assign
            var authSettings = ServiceFactory.CreateAuthSettings();
            var distributedCache = ServiceFactory.CreateDistributedCache();

            var handler = GetHander(
                authSettings: authSettings,
                distributedCache: distributedCache
                );

            // Act
            await handler.Handle(notification, CancellationToken.None);

            // Assert
            Assert.NotNull(distributedCache.SetCalledWith);
        }

        [Fact]
        public async Task Handle_CacheEntryExpiresLaterThanSession()
        {
            // Assign
            var authSettings = ServiceFactory.CreateAuthSettings();
            var distributedCache = ServiceFactory.CreateDistributedCache();

            var sessionExpires = DateTime.UtcNow.Add(authSettings.Value.SessionIdleTimeout);

            var handler = GetHander(
                authSettings: authSettings,
                distributedCache: distributedCache
                );

            // Act
            await handler.Handle(notification, CancellationToken.None);

            // Assert
            Assert.True(distributedCache.SetCalledWith.Options.AbsoluteExpiration > sessionExpires);
        }

        private KillUserProfileSessionEventHandler GetHander(IOptions<AuthSettings> authSettings = null, IDistributedCache distributedCache = null)
        {
            return new KillUserProfileSessionEventHandler(
                authSettings: authSettings ?? ServiceFactory.CreateAuthSettings(),
                distributedCache: distributedCache ?? ServiceFactory.CreateDistributedCache()
                );
        }
    }
}
