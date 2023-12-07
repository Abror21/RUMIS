using System;

namespace Izm.Rumis.Application.Exceptions
{
    public class TemplateEmptyException : Exception
    {
        public TemplateEmptyException()
            : base(DefaultMessage)
        {
        }

        public TemplateEmptyException(string code)
            : base(DefaultMessage)
        {
            this.Code = code;
        }

        public TemplateEmptyException(string code, Exception inner)
            : base(DefaultMessage, inner)
        {
            this.Code = code;
        }

        public const string DefaultMessage = "error.emptyTemplate";

        public string Code { get; }
    }
}
