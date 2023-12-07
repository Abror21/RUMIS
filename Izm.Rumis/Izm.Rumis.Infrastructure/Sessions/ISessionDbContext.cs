using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Sessions
{
    public interface ISessionDbContext
    {
        DbSet<Session> Sessions { get; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
