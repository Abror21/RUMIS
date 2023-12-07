using Izm.Rumis.Application.Common;
using System.Collections.Generic;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal sealed class SequenceServiceFake : ISequenceService
    {
        Dictionary<string, int> keyValuePairs = new Dictionary<string, int>();
        public long GetByKey(string key)
        {
            if (keyValuePairs.ContainsKey(key))
                keyValuePairs[key]++;
            else
                keyValuePairs[key] = 1;
            return keyValuePairs[key];
        }
    }

}

