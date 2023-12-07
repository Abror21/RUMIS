using Izm.Rumis.Infrastructure.Tests.Common;
using Izm.Rumis.Infrastructure.Vraa;
using Izm.Rumis.Infrastructure.Vraa.Exceptions;
using Izm.Rumis.Infrastructure.Vraa.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Infrastructure.Tests
{
    public sealed class VraaClientTests
    {
        [Fact]
        public async Task IntrospectAsync_Throws_Unsuccessful()
        {
            // Assign
            var handler = ServiceFactory.CreateHttpMessageHandler();

            handler.StatusCode = HttpStatusCode.BadRequest;

            using var http = GetHttpClient(handler);

            var vraaClient = GetClient(http);

            // Act & Assert
            var result = await Assert.ThrowsAsync<VraaClientException>(
                () => vraaClient.IntrospectAsync(string.Empty)
                );

            // Assert
            Assert.Equal(VraaClient.Error.IntrospectRequestFailed, result.Message);
        }

        [Fact]
        public async Task IntrospectAsync_Successful()
        {
            // Assign
            var handler = ServiceFactory.CreateHttpMessageHandler();

            var introspectResult = new IntrospectResult
            {
                Active = "Active",
                FirstName = "FirstName",
                LastName = "LastName",
                PrivatePersonalIdentifier = "PrivatePersonalIdentifier"
            };

            handler.Content = JsonSerializer.Serialize(introspectResult);

            using var http = GetHttpClient(handler);

            var vraaClient = GetClient(http);

            // Act & Assert
            var result = await vraaClient.IntrospectAsync(string.Empty);

            // Assert
            Assert.Equal(introspectResult.Active, result.Active);
            Assert.Equal(introspectResult.FirstName, result.FirstName);
            Assert.Equal(introspectResult.LastName, result.LastName);
            Assert.Equal(introspectResult.PrivatePersonalIdentifier, result.PrivatePersonalIdentifier);
        }

        private VraaClient GetClient(HttpClient http)
        {
            return new VraaClient(http);
        }

        private HttpClient GetHttpClient(HttpMessageHandler handler)
        {
            var http = new HttpClient(handler);

            http.BaseAddress = new Uri("http://test.test.test/");

            return http;
        }
    }
}
