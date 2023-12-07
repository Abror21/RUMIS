using Izm.Rumis.Application.Common;
using StackExchange.Redis;

namespace Izm.Rumis.Infrastructure
{
    public class RedisService : ISequenceService
    {
        private readonly IDatabase db;

        public RedisService(IDatabase db)
        {
            this.db = db;
        }

        public long GetByKey(string key = "default")
        {
            return db.StringIncrement(key);
        }
    }
}
