using Microsoft.AspNetCore.Http;


namespace Izm.Rumis.Api.Tests.Setup.Services
{
    public sealed class HttpContextAccessorFake : IHttpContextAccessor
    {
        public HttpContext HttpContext { get; set; } = new DefaultHttpContext();
    }
}
