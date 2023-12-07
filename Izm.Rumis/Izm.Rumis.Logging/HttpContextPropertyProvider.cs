using Microsoft.AspNetCore.Http;
using System;

namespace Izm.Rumis.Logging
{
    public class HttpContextPropertyProvider
    {
        public HttpContextPropertyProvider(IHttpContextAccessor httpContextAccessor, Func<HttpContext, string> getter)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.getter = getter;
        }

        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly Func<HttpContext, string> getter;

        public override string ToString()
        {
            var context = httpContextAccessor.HttpContext;
            return context == null ? "NULL" : getter(context);
        }
    }
}
