using Izm.Rumis.Infrastructure.Vraa.Exceptions;
using Izm.Rumis.Infrastructure.Vraa.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Vraa
{
    public interface IVraaClient
    {
        /// <summary>
        /// Introspect VRAA token.
        /// </summary>
        /// <param name="token">Token to introspect.</param>
        /// <returns>Introspect result data.</returns>
        Task<IntrospectResult> IntrospectAsync(string token);
    }

    public sealed class VraaClient : IVraaClient
    {
        private readonly HttpClient http;

        public VraaClient(HttpClient http)
        {
            this.http = http;
        }

        /// <inheritdoc/>
        /// <exception cref="VraaClientException"></exception>
        public async Task<IntrospectResult> IntrospectAsync(string token)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", token }
            });

            var response = await http.PostAsync("introspect", content);

            return !response.IsSuccessStatusCode
                ? throw new VraaClientException(Error.IntrospectRequestFailed)
                : await response.Content.ReadFromJsonAsync<IntrospectResult>();
        }

        public static class Error
        {
            public const string IntrospectRequestFailed = "vraa.introspectRequestFailed";
        }
    }
}
