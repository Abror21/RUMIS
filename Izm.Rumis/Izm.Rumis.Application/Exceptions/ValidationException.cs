using System;
using System.Collections.Generic;

namespace Izm.Rumis.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException()
            : base(DefaultMessage)
        {
        }

        public ValidationException(string message)
            : base(message)
        {
        }

        public ValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public ValidationException(IEnumerable<object> data)
            : base(DefaultMessage)
        {
            this.Data = data;
        }

        public ValidationException(IEnumerable<object> data, Exception inner)
            : base(DefaultMessage, inner)
        {
            this.Data = data;
        }

        public ValidationException(string message, IEnumerable<object> data)
            : base(message)
        {
            this.Data = data;
        }

        public ValidationException(string message, IEnumerable<object> data, Exception inner)
            : base(message, inner)
        {
            this.Data = data;
        }

        public const string DefaultMessage = "error.validation";

        public new IEnumerable<object> Data { get; }
    }
}
