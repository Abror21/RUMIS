using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Sessions
{
    public class SessionManagerOptions
    {
        public TimeSpan SessionIdleTimeout { get; set; } = TimeSpan.FromMinutes(15);
    }

    public interface ISessionManager
    {
        TimeSpan SessionIdleTimeout { get; }

        /// <summary>
        /// Add session activity trace.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task AddActivityTraceAsync(string sessionId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get session activity trace.
        /// </summary>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Trace value.</returns>
        Task<string> GetActivityTraceAsync(string sessionId, CancellationToken cancellationToken = default);
    }

    public sealed class SessionManager : ISessionManager
    {
        public TimeSpan SessionIdleTimeout => options.SessionIdleTimeout;

        private const string cachePrefix = "SessionManager_";

        private readonly SessionManagerOptions options;
        private readonly IDistributedCache distributedCache;

        public SessionManager(IOptions<SessionManagerOptions> options, IDistributedCache distributedCache)
        {
            this.options = options.Value;
            this.distributedCache = distributedCache;
        }

        /// <inheritdoc/>
        public Task AddActivityTraceAsync(string sessionId, CancellationToken cancellationToken = default) =>
            distributedCache.SetStringAsync($"{cachePrefix}{sessionId}", DateTime.UtcNow.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.Add(options.SessionIdleTimeout)
            }, cancellationToken);


        /// <inheritdoc/>
        public Task<string> GetActivityTraceAsync(string sessionId, CancellationToken cancellationToken = default) =>
            distributedCache.GetStringAsync($"{cachePrefix}{sessionId}", cancellationToken);
    }
}
