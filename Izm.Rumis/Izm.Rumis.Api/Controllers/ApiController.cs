using Izm.Rumis.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Izm.Rumis.Api.Controllers
{
    [ApiController]
    [Route("_api/[controller]")]
    public abstract class ApiController : ControllerBase
    {
        //private const string defaultLanguage = "lv";
        //private string currentLanguage;

        //protected string CurrentLanguage
        //{
        //    get
        //    {
        //        if (currentLanguage == null)
        //        {
        //            Request.Headers.TryGetValue("x-app-language", out var langHeader);
        //            var lang = langHeader.FirstOrDefault();

        //            currentLanguage = (string.IsNullOrEmpty(lang) ? defaultLanguage : lang).ToLower();
        //        }

        //        return currentLanguage;
        //    }
        //}

        protected bool IsEnglish(ICurrentUserService currentUser)
        {
            return currentUser.Language == "en";
        }
    }
}
