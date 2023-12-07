using Izm.Rumis.Api.Options;
using Microsoft.Extensions.Options;
using System;

namespace Izm.Rumis.Api.Tests.Setup.Options
{
    internal sealed class AuthUserProfileOptionsFake : IOptions<AuthUserProfileOptions>
    {
        public AuthUserProfileOptions Value => new AuthUserProfileOptions
        {
            TokenLifeTime = TimeSpan.FromMinutes(1),
            TokenSecurityKey = "Sqnr00ZcX6wNwxZzs633s5REEzywL3xw"
        };
    }
}
