
using Microsoft.AspNetCore.Mvc;

namespace Izm.Rumis.Api.Attributes
{
    public class StaticFileResponseCacheConfig
    {
        public static int Duration = 3600;
    }

    public class StaticFileResponseCacheAttribute : ResponseCacheAttribute
    {
        public StaticFileResponseCacheAttribute()
        {
            this.Duration = StaticFileResponseCacheConfig.Duration;
        }
    }
}
