using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal sealed class SessionFake : ISession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool IsAvailable { get; set; } = true;
        public IEnumerable<string> Keys { get; set; } = Array.Empty<string>();
        public Dictionary<string, byte[]> Cache { get; set; } = new();
        public bool ClearCalled { get; set; } = false;

        public void Clear()
        {
            ClearCalled = true;

            return;
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            return;
        }

        public void Set(string key, byte[] value)
        {
            Cache.Add(key, value);

            return;
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[] value)
        {
            return Cache.TryGetValue(key, out value);
        }
    }
}
