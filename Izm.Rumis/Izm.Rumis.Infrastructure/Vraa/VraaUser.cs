using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Izm.Rumis.Infrastructure.Vraa
{
    public interface IVraaUser
    {
        string FirstName { get; }
        string LastName { get; }
        string PrivatePersonalIdentifier { get; }
    }

    public sealed class VraaUser : IVraaUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PrivatePersonalIdentifier { get; set; }

        public VraaUser(IHttpContextAccessor httpContextAccessor)
        {
            var ctx = httpContextAccessor.HttpContext;

            if (ctx == null)
                return;

            var user = ctx.User;

            if (user != null && user.Identity.IsAuthenticated)
            {
                FirstName = user.FindFirstValue(ClaimTypes.GivenName);
                LastName = user.FindFirstValue(ClaimTypes.Name);
                PrivatePersonalIdentifier = user.FindFirstValue(ClaimTypesExtensions.PrivatePersonalIdentifier);
            }
        }
    }
}
