using System;

namespace Izm.Rumis.Api.Models
{
    public class IdValue<TId, TValue>
    {
        public TId Id { get; set; }
        public TValue Value { get; set; }
    }

    public class ClassifierValue : IdValue<Guid, string>
    {
        public string Type { get; set; }
        public string Code { get; set; }
        // this is internal by intention, since payload may contain sensitive data
        internal string Payload { get; set; }
    }
}
