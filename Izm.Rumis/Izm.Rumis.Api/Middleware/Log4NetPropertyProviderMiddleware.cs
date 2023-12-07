using Izm.Rumis.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Middleware
{
    public class Log4NetPropertyProviderMiddleware
    {
        private readonly RequestDelegate next;

        public Log4NetPropertyProviderMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var currentUser = context.RequestServices.GetRequiredService<ICurrentUserService>();
            var currentUserProfile = context.RequestServices.GetRequiredService<ICurrentUserProfileService>();

            log4net.GlobalContext.Properties["userid"] = currentUser.Id == Guid.Empty ? null : currentUser.Id;
            log4net.GlobalContext.Properties["personid"] = currentUser.PersonId == Guid.Empty ? null : currentUser.PersonId;
            log4net.GlobalContext.Properties["userprofileid"] = currentUserProfile.Id == Guid.Empty ? null : currentUserProfile.Id;
            log4net.GlobalContext.Properties["sessionid"] = context.User.Identity.IsAuthenticated ? context.Session.Id : null;
            log4net.GlobalContext.Properties["educationalinstitutionid"] = currentUserProfile.Id == Guid.Empty ? null : currentUserProfile.EducationalInstitutionId;
            log4net.GlobalContext.Properties["supervisorid"] = currentUserProfile.Id == Guid.Empty ? null : currentUserProfile.SupervisorId;

            await next(context);
        }
    }
}
