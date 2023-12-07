using System;

namespace Izm.Rumis.Api.Options
{
    public sealed class AuthUserProfileOptions
    {
        public TimeSpan TokenLifeTime { get; set; } = TimeSpan.FromMinutes(30);
        public string TokenSecurityKey { get; set; }
    }
}
