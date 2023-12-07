using Microsoft.EntityFrameworkCore;

namespace Izm.Rumis.Infrastructure.Logging
{
    public interface ILogDbContext
    {
        DbSet<Log> Log { get; set; }
    }
}
