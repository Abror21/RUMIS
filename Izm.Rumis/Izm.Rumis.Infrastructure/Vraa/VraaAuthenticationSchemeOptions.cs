using Microsoft.AspNetCore.Authentication;

namespace Izm.Rumis.Infrastructure.Vraa
{
    public class VraaAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public string IntrospectUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
