using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Izm.Rumis.Api.Core
{
    /// <summary>
    /// JSON web token manager
    /// </summary>
    public class JwtManager
    {
        /// <summary>
        /// Generate user access token.
        /// </summary>
        /// <param name="claims">User claims.</param>
        /// <param name="securityKey"></param>
        /// <param name="lifeTime">Token lifetime.</param>
        /// <returns></returns>
        public static AccessToken GenerateAccessToken(IEnumerable<Claim> claims, string securityKey, TimeSpan lifeTime) =>
            GenerateAccessToken(claims, securityKey, (int)lifeTime.TotalMinutes);

        /// <summary>
        /// Generate user access token.
        /// </summary>
        /// <param name="claims">User claims.</param>
        /// <param name="securityKey"></param>
        /// <param name="expiresInMinutes">Token lifespan in minutes. If set to null, token is meant to never expire.</param>
        /// <returns></returns>
        public static AccessToken GenerateAccessToken(IEnumerable<Claim> claims, string securityKey, int expiresInMinutes = 10)
            => GenerateAccessToken(claims, securityKey, DateTime.UtcNow.AddMinutes(expiresInMinutes));

        /// <summary>
        /// Generate user access token.
        /// </summary>
        /// <param name="claims">User claims.</param>
        /// <param name="securityKey"></param>
        /// <param name="expiresInMinutes">Token lifespan in minutes. If set to null, token is meant to never expire.</param>
        /// <returns></returns>
        public static AccessToken GenerateAccessToken(IEnumerable<Claim> claims, string securityKey, DateTime? expires)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(securityKey);
            var identity = new ClaimsIdentity(claims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AccessToken
            {
                Token = tokenHandler.WriteToken(token),
                Expires = expires
            };
        }

        /// <summary>
        /// Generate user access token.
        /// </summary>
        /// <param name="accessToken">Access token to decode.</param>
        /// <param name="securityKey"></param>
        /// <returns></returns>
        public static ClaimsPrincipal DecodeAccessToken(string accessToken, string securityKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(securityKey);
            var identity = tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) =>
                {
                    var utc = DateTime.UtcNow;

                    return (notBefore == null || notBefore.Value.ToUniversalTime() <= utc)
                    && (expires == null || expires.Value.ToUniversalTime() > utc);
                },
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero // invalidate token immediatelly when expired
            }, out var token);

            return identity;
        }

        /// <summary>
        /// Generate user refresh token.
        /// </summary>
        /// <returns></returns>
        public static string GenerateRefreshToken()
        {
            using (var rng = RandomNumberGenerator.Create("System.Security.Cryptography.RandomNumberGenerator"))
            {
                var randomBytes = new byte[64];
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }

        public class AccessToken
        {
            public string Token { get; set; }
            public DateTime? Expires { get; set; }
        }
    }
}
