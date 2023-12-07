using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate next;

        public AuthorizationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // process access token
            //throw new UnauthorizedAccessException("token is not valid/expired/etc");

            await next.Invoke(context);

            //var validator = new JwtSecurityTokenHandler();

            //if (!validator.CanReadToken(queryString.Get("token")))
            //    throw new UnauthorizedAccessException("token is not readable");
            //try
            //{
            //    //trying to parse the token 
            //    var principal =
            //        validator.ValidateToken(queryString.Get("token"), validationParameters, out var validatedToken);
            //    if (principal.HasClaim(c => c.Type == ClaimTypes.UserData))
            //    {
            //        var userData = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;

            //        //setting AuthData to be used for later usages in other middlewares
            //        //as well as the controller which is in the MVC middleware 
            //        context.Items["AuthData"] = JsonConvert.DeserializeObject<LoginModel>(userData);

            //        //authorizing the http context 
            //        context.User.AddIdentity((ClaimsIdentity)principal.Identity);

            //        //invoking the next middleware 
            //        await _next.Invoke(context);
            //    }
            //}
            //catch (Exception e)
            //{
            //    throw;
            //}
        }
    }
}
