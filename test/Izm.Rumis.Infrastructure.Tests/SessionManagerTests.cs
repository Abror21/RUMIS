using Izm.Rumis.Infrastructure.Sessions;
using Izm.Rumis.Infrastructure.Tests.Common;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Infrastructure.Tests
{
    public sealed class SessionManagerTests
    {
        [Fact]
        public async Task AddActivityTraceAsync_Succeeds()
        {
            // Assign
            var distributedCache = ServiceFactory.CreateDistributedCache();
            var options = new SessionManagerOptions
            {
                SessionIdleTimeout = TimeSpan.FromMinutes(1)
            };

            var sessionManager = GetManager(
                options: options,
                distributedCache: distributedCache
                );

            // Act
            await sessionManager.AddActivityTraceAsync(Guid.NewGuid().ToString());

            // Assert
            Assert.Single(distributedCache.Storage);
            Assert.True(distributedCache.SetCalledWith.Options.AbsoluteExpiration < DateTimeOffset.UtcNow.Add(options.SessionIdleTimeout));
        }

        [Fact]
        public async Task GetActivityTraceAsync_Succeeds()
        {
            // Assign
            var sessionId = Guid.NewGuid().ToString();

            var distributedCache = ServiceFactory.CreateDistributedCache();

            distributedCache.Storage.Add($"SessionManager_{sessionId}", new byte[] { 1, 2, 3 });

            var sessionManager = GetManager(
                distributedCache: distributedCache
                );

            // Act
            var result = await sessionManager.GetActivityTraceAsync(sessionId);

            // Assert
            Assert.NotNull(result);
        }

        private SessionManager GetManager(SessionManagerOptions options = null, IDistributedCache distributedCache = null)
        {
            return new SessionManager(
                options: Options.Create(options ?? new SessionManagerOptions()),
                distributedCache: distributedCache ?? ServiceFactory.CreateDistributedCache()
                );
        }
    }
}
