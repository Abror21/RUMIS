using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal sealed class HttpMessageHandlerFake : HttpMessageHandler
    {
        public string Data { get; set; }
        public string Content { get; set; } = JsonConvert.SerializeObject("Content");
        public HttpMethod Method { get; set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content != null)
                Data = await request.Content.ReadAsStringAsync();

            Method = request.Method;

            return new HttpResponseMessage() { StatusCode = StatusCode, Content = new StringContent(Content) };
        }
    }
}
