using AutoMapper.Internal;
using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Core;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Helpers;
using Izm.Rumis.Api.Options;
using Izm.Rumis.Api.Services;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public sealed class CurrentUserProfileServiceTests
    {
        [Fact]
        public void Constructor_Succeeds_Initialized()
        {
            // Assign
            var sessionId = Guid.NewGuid();
            var sessionCreated = DateTime.Now.ToString();
            var userId = Guid.NewGuid();
            var userProfileId = Guid.NewGuid();
            const int educationalInstitutionId = 1;
            const int supervisorId = 2;
            const string role = "someRole";
            const UserProfileType type = UserProfileType.Country;
            var permissions = new string[] { "p1", "p2" };

            var options = ServiceFactory.CreateAuthUserProfileOptions();
            var httpContextAccessor = ServiceFactory.CreateHttpContextAccessor();

            var identity = new ClaimsIdentity("test");
            identity.AddClaim(ClaimHelper.CreateClaim(ClaimTypes.NameIdentifier, userId));

            httpContextAccessor.HttpContext.User = new ClaimsPrincipal(identity);

            var session = ServiceFactory.CreateSession();
            session.Id = sessionId.ToString();

            session.SetString(SessionKey.Created, sessionCreated);

            httpContextAccessor.HttpContext.Session = session;

            var profileClaims = new List<Claim>()
            {
                ClaimHelper.CreateClaim(ClaimTypes.NameIdentifier, userId),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisUserProfileEducationalInstitutionIdentifier, educationalInstitutionId),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisUserProfileIdentifier, userProfileId),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisUserProfileSupervisorIdentifier, supervisorId),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisUserProfileRole, role),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisUserProfileType, type),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisSessionId, sessionId),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisSessionCreated, sessionCreated)
            };

            permissions.ForAll(t => profileClaims.Add(ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisUserProfileRolePermission, t)));

            httpContextAccessor.HttpContext.Request.Headers.Add(
                CurrentUserProfileService.HeaderName, JwtManager.GenerateAccessToken(profileClaims, options.Value.TokenSecurityKey).Token
                );

            // Act
            var result = GetService(options, httpContextAccessor);

            // Assert
            Assert.Equal(userId, result.UserId);
            Assert.Equal(userProfileId, result.Id);
            Assert.Equal(educationalInstitutionId, result.EducationalInstitutionId);
            Assert.Equal(supervisorId, result.SupervisorId);
            Assert.Equal(role, result.Role);
            Assert.Equal(type, result.Type);
            Assert.Equal(permissions.Length, result.Permissions.Count());
            Assert.DoesNotContain(result.Permissions, t => !permissions.Contains(t));
            Assert.True(result.IsInitialized);
        }

        [Fact]
        public void Constructor_Succeeds_NotInitialized_EmptyToken()
        {
            // Assign
            var options = ServiceFactory.CreateAuthUserProfileOptions();
            var httpContextAccessor = ServiceFactory.CreateHttpContextAccessor();

            var identity = new ClaimsIdentity("test");

            httpContextAccessor.HttpContext.User = new ClaimsPrincipal(identity);

            httpContextAccessor.HttpContext.Request.Headers.Add(
                CurrentUserProfileService.HeaderName, string.Empty
                );


            // Act
            var result = GetService(options, httpContextAccessor);

            // Assert
            Assert.False(result.IsInitialized);
        }

        [Fact]
        public void Constructor_Succeeds_NotInitialized_NotAuthenticated()
        {
            // Assign
            var options = ServiceFactory.CreateAuthUserProfileOptions();
            var httpContextAccessor = ServiceFactory.CreateHttpContextAccessor();

            // Act
            var result = GetService(options, httpContextAccessor);

            // Assert
            Assert.False(result.IsInitialized);
        }

        [Fact]
        public void Constructor_Succeeds_NotInitialized_NoHeader()
        {
            // Assign
            var options = ServiceFactory.CreateAuthUserProfileOptions();
            var httpContextAccessor = ServiceFactory.CreateHttpContextAccessor();

            var identity = new ClaimsIdentity("test");

            httpContextAccessor.HttpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = GetService(options, httpContextAccessor);

            // Assert
            Assert.False(result.IsInitialized);
        }

        [Fact]
        public void Constructor_Throws_InvalidTokenProvided_SessionCreatedMismatch()
        {
            // Assign
            var sessionId = Guid.NewGuid().ToString();
            var options = ServiceFactory.CreateAuthUserProfileOptions();
            var httpContextAccessor = ServiceFactory.CreateHttpContextAccessor();

            var identity = new ClaimsIdentity("test");
            identity.AddClaim(ClaimHelper.CreateClaim(ClaimTypes.NameIdentifier, Guid.NewGuid()));

            httpContextAccessor.HttpContext.User = new ClaimsPrincipal(identity);

            var session = ServiceFactory.CreateSession();
            session.Id = sessionId;

            session.SetString(SessionKey.Created, DateTime.Now.ToBinary().ToString());

            httpContextAccessor.HttpContext.Session = session;

            var profileClaims = new List<Claim>()
            {
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisSessionId, sessionId),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisSessionCreated, DateTime.Now.ToBinary().ToString())
            };

            httpContextAccessor.HttpContext.Request.Headers.Add(
                CurrentUserProfileService.HeaderName, JwtManager.GenerateAccessToken(profileClaims, options.Value.TokenSecurityKey).Token
                );

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => GetService(options, httpContextAccessor));

            // Assert
            Assert.Equal(CurrentUserProfileService.Error.InvalidTokenProvided, result.Message);
        }

        [Fact]
        public void Constructor_Throws_InvalidTokenProvided_SessionIdMismatch()
        {
            // Assign
            var sessionCreated = DateTime.Now.ToString();

            var options = ServiceFactory.CreateAuthUserProfileOptions();
            var httpContextAccessor = ServiceFactory.CreateHttpContextAccessor();

            var identity = new ClaimsIdentity("test");
            identity.AddClaim(ClaimHelper.CreateClaim(ClaimTypes.NameIdentifier, Guid.NewGuid()));

            httpContextAccessor.HttpContext.User = new ClaimsPrincipal(identity);

            var session = ServiceFactory.CreateSession();
            session.Id = Guid.NewGuid().ToString();

            session.SetString(SessionKey.Created, sessionCreated);

            httpContextAccessor.HttpContext.Session = session;

            var profileClaims = new List<Claim>()
            {
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisSessionId, Guid.NewGuid()),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisSessionCreated, sessionCreated)
            };

            httpContextAccessor.HttpContext.Request.Headers.Add(
                CurrentUserProfileService.HeaderName, JwtManager.GenerateAccessToken(profileClaims, options.Value.TokenSecurityKey).Token
                );

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => GetService(options, httpContextAccessor));

            // Assert
            Assert.Equal(CurrentUserProfileService.Error.InvalidTokenProvided, result.Message);
        }

        [Fact]
        public void Constructor_Throws_InvalidUserProfile_UserIdMismatch()
        {
            // Assign
            var sessionId = Guid.NewGuid();
            var sessionCreated = DateTime.Now.ToString();
            var userId = Guid.NewGuid();
            var userIdInProfile = Guid.NewGuid();

            var options = ServiceFactory.CreateAuthUserProfileOptions();
            var httpContextAccessor = ServiceFactory.CreateHttpContextAccessor();

            var identity = new ClaimsIdentity("test");
            identity.AddClaim(ClaimHelper.CreateClaim(ClaimTypes.NameIdentifier, userId));

            httpContextAccessor.HttpContext.User = new ClaimsPrincipal(identity);

            var session = ServiceFactory.CreateSession();
            session.Id = sessionId.ToString();

            session.SetString(SessionKey.Created, sessionCreated);

            httpContextAccessor.HttpContext.Session = session;

            var profileClaims = new List<Claim>()
            {
                ClaimHelper.CreateClaim(ClaimTypes.NameIdentifier, userIdInProfile),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisSessionId, sessionId),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisSessionCreated, sessionCreated)
            };

            httpContextAccessor.HttpContext.Request.Headers.Add(
                CurrentUserProfileService.HeaderName, JwtManager.GenerateAccessToken(profileClaims, options.Value.TokenSecurityKey).Token
                );

            // Act & Assert
            var result = Assert.Throws<InvalidUserProfileException>(() => GetService(options, httpContextAccessor));

            // Assert
            Assert.Equal(userId, result.UserId);
            Assert.Equal(userIdInProfile, result.UserIdInProfile);
        }

        private CurrentUserProfileService GetService(
            IOptions<AuthUserProfileOptions> options = null,
            IHttpContextAccessor httpContextAccessor = null
            )
        {
            return new CurrentUserProfileService(
                options ?? ServiceFactory.CreateAuthUserProfileOptions(),
                httpContextAccessor ?? ServiceFactory.CreateHttpContextAccessor()
                );
        }
    }
}
