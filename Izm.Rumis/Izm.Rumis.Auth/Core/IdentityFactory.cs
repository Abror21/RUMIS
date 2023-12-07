using System;
using System.Security.Claims;

namespace Izm.Rumis.Auth.Core
{
    internal static class IdentityFactory
    {
        public static ClaimsIdentity CreateIdentity(IdentityClaims claims)
        {
            var user = new ClaimsIdentity();

            Action<string, string> addClaim = (type, value) =>
            {
                var claim = new Claim(type, value);
                user.AddClaim(claim);
            };

            addClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", claims.NameIdentifier);
            addClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", claims.Email);
            addClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", claims.Role);
            addClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", claims.GivenName);
            addClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", claims.Surname);
            addClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", claims.Name);

            return user;
        }

        public static ClaimsIdentity CreateDevIdentity()
        {
            return CreateIdentity(new IdentityClaims
            {
                NameIdentifier = "dev@dzc.lv",
                Email = "dev@dzc.lv",
                Role = "dzc\\dev",
                GivenName = "Admin",
                Surname = "Dev",
                Name = "Admin Dev"
            });
        }

        public static ClaimsIdentity CreateTestIdentity(int id)
        {
            return CreateIdentity(new IdentityClaims
            {
                NameIdentifier = $"test{id}@dzc.lv",
                Email = $"test{id}@dzc.lv",
                Role = $"dzc\\test",
                GivenName = "Test",
                Surname = $"{id}",
                Name = $"Test {id}"
            });
        }
    }
}
