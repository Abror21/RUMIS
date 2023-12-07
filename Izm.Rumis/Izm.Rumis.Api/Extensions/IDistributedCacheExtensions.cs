using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Extensions
{
    public static class IDistributedCacheExtensions
    {
        public static async Task<T> GetFromJsonAsync<T>(this IDistributedCache distributedCache, string key)
        {
            var stringValue = await distributedCache.GetStringAsync(key);

            return string.IsNullOrEmpty(stringValue)
                ? default
                : JsonSerializer.Deserialize<T>(await distributedCache.GetStringAsync(key));
        }

        public static Task SetAsync(this IDistributedCache distributedCache, string key, object value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return distributedCache.SetStringAsync(key, JsonSerializer.Serialize(value), options, token);
        }
    }
}
