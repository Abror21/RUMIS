using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Infrastructure.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Middleware
{
    /// <summary>
    /// Authenticate user based on session.
    /// </summary>
    /// <remarks>
    /// This can be combined with Authentication middleware into an extended JWT authentication scheme.
    /// </remarks>
    public class SessionAuthenticationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ISessionManager sessionManager;

        public SessionAuthenticationMiddleware(RequestDelegate next, ISessionManager sessionManager)
        {
            this.next = next;
            this.sessionManager = sessionManager;
        }

        public async Task Invoke(HttpContext context)
        {
            var allowAnnoymous = context.GetEndpoint()?.Metadata?
                .GetMetadata<AllowAnonymousAttribute>();

            Task activityTraceTask = null;

            if (allowAnnoymous == null && context.User.Identity.IsAuthenticated)
            {
                var sessionId = context.User.FindFirstValue(ClaimTypesExtensions.RumisSessionId);
                var sessionCreated = context.User.FindFirstValue(ClaimTypesExtensions.RumisSessionCreated);

                if (sessionId != context.Session.Id || sessionCreated != context.Session.GetString(SessionKey.Created))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                    return;
                }

                activityTraceTask = sessionManager.AddActivityTraceAsync(context.Session.Id);
            }

            await next(context);

            if (activityTraceTask != null)
                await activityTraceTask;
        }
    }
}
