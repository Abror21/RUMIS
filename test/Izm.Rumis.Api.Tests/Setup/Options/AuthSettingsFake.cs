using Izm.Rumis.Api.Options;
using Microsoft.Extensions.Options;

namespace Izm.Rumis.Api.Tests.Setup.Options
{
    internal class AuthSettingsFake : IOptions<AuthSettings>
    {
        public AuthSettings Settings { get; set; } = new AuthSettings
        {
            AppUrl = "/",
            ExternalUrl = "/external-auth",
            ExternalProvider = "adfs/test",
            TicketPassword = "test",
            TokenSecurityKey = "thisisverysecretkey"
        };

        public AuthSettings Value => Settings;
    }
}
