using Izm.Rumis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Identity
{
    public interface IIdentityDbContext
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        DbSet<IdentityUser> IdentityUsers { get; }
        DbSet<IdentityUserLogin> IdentityUserLogins { get; }
        DbSet<User> Users { get; }
        DbSet<UserProfile> UserProfiles { get; }
        DbSet<Person> Persons { get; }
    }
}
