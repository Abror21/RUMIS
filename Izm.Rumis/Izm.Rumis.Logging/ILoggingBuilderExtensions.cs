using Microsoft.Extensions.Logging;

namespace Izm.Rumis.Logging
{
    public static class ILoggingBuilderExtensions
    {
        public static void AddCustomLogging(this ILoggingBuilder builder)
        {
            builder.AddLog4Net();
        }
    }
}
