using Microsoft.Extensions.Logging;
using System;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal class LoggerFake<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine(state);

            if (exception != null)
                Console.WriteLine(exception);
        }
    }
}
