using Izm.Rumis.Application.Common;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal sealed class SequenceServiceFake : ISequenceService
    {
        public long GetByKey(string key)
        {
            return 1;
        }
    }

}

