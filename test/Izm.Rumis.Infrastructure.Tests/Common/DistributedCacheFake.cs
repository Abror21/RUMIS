﻿using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal sealed class DistributedCacheFake : IDistributedCache
    {
        public Dictionary<string, byte[]> Storage { get; set; } = new();
        public SetCalledWith SetCalledWith { get; set; } = null;

        public byte[] Get(string key)
        {
            return Storage.TryGetValue(key, out byte[] value) ? value : null;
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return Task.Run(() => Get(key), token);
        }

        public void Refresh(string key)
        {
            return;
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            Storage.Remove(key);

            return;
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return Task.Run(() => Remove(key), token);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            Storage[key] = value;

            SetCalledWith = new SetCalledWith(key, value, options);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return Task.Run(() => Set(key, value, options), token);
        }
    }

    internal record SetCalledWith(string Key, byte[] Value, DistributedCacheEntryOptions Options);
}
