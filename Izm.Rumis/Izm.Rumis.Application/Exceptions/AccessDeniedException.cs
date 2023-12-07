using System;

namespace Izm.Rumis.Application.Exceptions
{
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException()
            : base(DefaultMessage)
        {
        }

        public AccessDeniedException(string message)
            : base(message)
        {
        }

        public AccessDeniedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public const string DefaultMessage = "error.accessDenied";
    }
}
