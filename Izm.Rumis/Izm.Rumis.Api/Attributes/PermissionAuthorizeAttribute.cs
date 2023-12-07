using Izm.Rumis.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Izm.Rumis.Api.Attributes
{
    /// <summary>
    /// Authorization attribute to authorize user by permissions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PermissionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private IEnumerable<string> permissions = new List<string>();

        public PermissionAuthorizeAttribute(params string[] permissions)
        {
            this.permissions = permissions;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.Any(t => t.GetType() == typeof(AllowAnonymousAttribute)))
                return;

            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var currentUserProfile = context.HttpContext.RequestServices.GetService<ICurrentUserProfileService>();

            foreach (var p in permissions)
            {
                if (currentUserProfile.Permissions.Contains(p))
                    return;
            }

            context.Result = new ForbidResult();

            return;
        }
    }
}
