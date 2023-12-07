using System;

namespace Izm.Rumis.Application.Exceptions
{
    public class ReadOnlyException : Exception
    {
        public ReadOnlyException()
            : base(DefaultMessage)
        {
        }

        public ReadOnlyException(string message)
            : base(message)
        {
        }

        public ReadOnlyException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public const string DefaultMessage = "error.readOnly";
    }
}
