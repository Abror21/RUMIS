using Izm.Rumis.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Izm.Rumis.Infrastructure.Logging
{
    public interface ILogService
    {
        /// <summary>
        /// Get log entries.
        /// </summary>
        /// <returns></returns>
        SetQuery<Log> Get();
    }

    public class LogService : ILogService
    {
        private readonly ILogDbContext db;

        public LogService(ILogDbContext db)
        {
            this.db = db;
        }

        /// <inheritdoc/>
        public SetQuery<Log> Get()
        {
            return new SetQuery<Log>(db.Log.AsNoTracking());
        }
    }
}
