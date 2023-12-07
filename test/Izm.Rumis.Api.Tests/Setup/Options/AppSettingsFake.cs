using Izm.Rumis.Api.Options;
using Microsoft.Extensions.Options;

namespace Izm.Rumis.Api.Tests.Setup.Options
{
    internal class AppSettingsFake : IOptions<AppSettings>
    {
        public AppSettings Value => new AppSettings
        {
            AppUrl = "/"
        };
    }
}
