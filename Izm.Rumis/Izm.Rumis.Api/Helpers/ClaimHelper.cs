using System.Security.Claims;

namespace Izm.Rumis.Api.Helpers
{
    public static class ClaimHelper
    {
        public static Claim CreateClaim(string type, string value)
        {
            return new Claim(type, value);
        }

        public static Claim CreateClaim<T>(string type, T value)
        {
            return CreateClaim(type, value.ToString());
        }
    }
}
