using Izm.Rumis.Infrastructure.EAddress.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.EAddress
{
    public sealed class EAddressClient : IEAddressClient
    {
        private readonly HttpClient http;

        public EAddressClient(HttpClient http)
        {
            this.http = http;
        }

        public async Task<string> SendMessageAsync(EAddressSendMessageRequest request, CancellationToken cancellationToken = default)
        {
            var httpResponse = await http.PostAsJsonAsync("sendMessage", request, cancellationToken);

            return !httpResponse.IsSuccessStatusCode
                ? throw new Exception($"EAddress error! {await httpResponse.Content.ReadAsStringAsync()}")
                : await httpResponse.Content.ReadFromJsonAsync<string>();
        }

        public async Task<EAddressValidateNaturalPersonResponse> ValidateNaturalPersonAsync(string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            var httpResponse = await http.PostAsJsonAsync("validateNaturalPerson", privatePersonalIdentifier, cancellationToken);

            return !httpResponse.IsSuccessStatusCode
                ? throw new Exception($"EAddress error! {await httpResponse.Content.ReadAsStringAsync()}")
                : await httpResponse.Content.ReadFromJsonAsync<EAddressValidateNaturalPersonResponse>();
        }
    }
}
