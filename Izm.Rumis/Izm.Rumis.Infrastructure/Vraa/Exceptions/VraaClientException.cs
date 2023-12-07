using System;

namespace Izm.Rumis.Infrastructure.Vraa.Exceptions
{
    public class VraaClientException : Exception
    {
        public const string DefaultMessage = "vraa.error";

        public VraaClientException() : base(DefaultMessage) { }
        public VraaClientException(Exception ex) : base(DefaultMessage, ex) { }
        public VraaClientException(string message) : base(message) { }
        public VraaClientException(string message, Exception ex) : base(message, ex) { }
    }
}
