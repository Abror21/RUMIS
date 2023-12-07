using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Izm.Rumis.Api.Tests.Setup.Common
{
    internal class RequestCookieCollectionFake : IRequestCookieCollection
    {
        public RequestCookieCollectionFake(params KeyValuePair<string, string>[] data)
        {
            this.data.AddRange(data);
        }

        private readonly List<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>>();

        public string this[string key] => data.Where(t => t.Key == key).Select(t => t.Value).FirstOrDefault();

        public int Count => data.Count;

        public ICollection<string> Keys => throw new NotImplementedException();

        public bool ContainsKey(string key)
        {
            return data.Any(t => t.Key == key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        public bool TryGetValue(string key, out string value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }
    }
}
