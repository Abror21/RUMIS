using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Infrastructure.Tests.Common;
using Izm.Rumis.Infrastructure.Vraa;
using Izm.Rumis.Infrastructure.Vraa.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Infrastructure.Tests
{
    public sealed class VraaAuthenticationHandlerTests
    {
        [Fact]
        public async Task Authenticate_Fails_HeaderNotFound()
        {
            // Assign
            var handler = GetHandler();

            await SetupAsync(handler);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            Assert.Equal(VraaAuthenticationHandler.Error.HeaderNotFound, result.Failure.Message);
        }

        [Fact]
        public async Task Authenticate_Fails_TokenDoesNotIdentifyPerson()
        {
            // Assign
            var vraaClient = ServiceFactory.CreateVraaClientFake();

            vraaClient.IntrospectResult = new IntrospectResult
            {
                Active = true.ToString()
            };

            var handler = GetHandler(vraaClient: vraaClient);

            var context = new DefaultHttpContext();

            context.Request.Headers.Add(HeaderNames.Authorization, "someValue");

            await SetupAsync(handler, context);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            Assert.Equal(VraaAuthenticationHandler.Error.TokenDoesNotIdentifyPerson, result.Failure.Message);
        }

        [Fact]
        public async Task Authenticate_Fails_TokenNotActive()
        {
            // Assign
            var vraaClient = ServiceFactory.CreateVraaClientFake();

            vraaClient.IntrospectResult = new IntrospectResult
            {
                Active = false.ToString()
            };

            var handler = GetHandler(vraaClient: vraaClient);

            var context = new DefaultHttpContext();

            context.Request.Headers.Add(HeaderNames.Authorization, "someValue");

            await SetupAsync(handler, context);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            Assert.Equal(VraaAuthenticationHandler.Error.TokenNotActive, result.Failure.Message);
        }

        [Fact]
        public async Task Authenticate_Fails_TokenNotFound()
        {
            // Assign
            var handler = GetHandler();

            var context = new DefaultHttpContext();

            context.Request.Headers.Add(HeaderNames.Authorization, string.Empty);

            await SetupAsync(handler, context);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            Assert.Equal(VraaAuthenticationHandler.Error.TokenNotFound, result.Failure.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_Succeeds()
        {
            // Assign
            var vraaClient = ServiceFactory.CreateVraaClientFake();

            vraaClient.IntrospectResult = new IntrospectResult
            {
                Active = true.ToString(),
                PrivatePersonalIdentifier = "00000000000",
                FirstName = "FirstName",
                LastName = "LastName"
            };

            var gdprAuditService = ServiceFactory.CreateGdprAuditService();

            var handler = GetHandler(
                vraaClient: vraaClient,
                gdprAuditService: gdprAuditService
                );

            var context = new DefaultHttpContext();

            context.Request.Headers.Add(HeaderNames.Authorization, "someValue");

            await SetupAsync(handler, context);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(vraaClient.IntrospectResult.FirstName, result.Principal.FindFirstValue(ClaimTypes.GivenName));
            Assert.Equal(vraaClient.IntrospectResult.LastName, result.Principal.FindFirstValue(ClaimTypes.Surname));
            Assert.Equal(vraaClient.IntrospectResult.PrivatePersonalIdentifier, result.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier"));
            Assert.NotNull(gdprAuditService.TraceAsyncCalledWith);
        }

        private VraaAuthenticationHandler GetHandler(OptionsMonitor optionsMonitor = null, IVraaClient vraaClient = null, IGdprAuditService gdprAuditService = null)
        {
            return new VraaAuthenticationHandler(
                options: optionsMonitor ?? new OptionsMonitor(),
                logger: new NullLoggerFactory(),
                encoder: UrlEncoder.Create(),
                clock: new SystemClock(),
                vraaClient: vraaClient ?? ServiceFactory.CreateVraaClientFake(),
                gdprAuditService: gdprAuditService ?? ServiceFactory.CreateGdprAuditService()
                );
        }

        private async Task SetupAsync(VraaAuthenticationHandler handler, HttpContext context = null)
        {
            context = context ?? new DefaultHttpContext();

            await handler.InitializeAsync(new AuthenticationScheme(VraaDefaults.AuthenticationScheme, VraaDefaults.AuthenticationScheme, typeof(VraaAuthenticationHandler)), context);
        }

        private class OptionsMonitor : IOptionsMonitor<VraaAuthenticationSchemeOptions>
        {
            public VraaAuthenticationSchemeOptions CurrentValue { get; set; } = new VraaAuthenticationSchemeOptions();

            public VraaAuthenticationSchemeOptions Get(string name) => CurrentValue;

            public IDisposable OnChange(Action<VraaAuthenticationSchemeOptions, string> listener)
            {
                throw new NotImplementedException();
            }
        }
    }
}
