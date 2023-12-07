using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Izm.Rumis.Api.Attributes
{
    /// <summary>
    /// Authorization attribute to allow only annonymous users.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AnonymousOnlyAttribute : Attribute, IAuthorizationFilter, IAllowAnonymous
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
                context.Result = new BadRequestObjectResult("User must not log out first.");
        }
    }
}
