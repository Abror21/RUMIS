using Izm.Rumis.Infrastructure.Sessions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal sealed class SessionManagerFake : ISessionManager
    {
        public string ActivityTrace { get; set; } = null;
        public TimeSpan SessionIdleTimeout { get; set; } = TimeSpan.FromMinutes(15);
        public string AddActivityTraceCalledWith { get; set; } = null;
        public string GetActivityTraceCalledWith { get; set; } = null;

        public Task AddActivityTraceAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            AddActivityTraceCalledWith = sessionId;

            return Task.CompletedTask;
        }

        public Task<string> GetActivityTraceAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            GetActivityTraceCalledWith = sessionId;

            return Task.FromResult(ActivityTrace);
        }
    }
}
