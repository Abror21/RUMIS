using Izm.Rumis.Application.Common;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(IAppDbContext db)
        {
            await EnsureRoleAsync(db, RoleCode.Administrator, RoleName.Administrator, Permission.All);

            await db.SaveChangesAsync();
        }

        private static async Task EnsureRoleAsync(IAppDbContext db, string code, string name, IEnumerable<string> permissions)
        {
            var role = await db.Roles
                .Include(t => t.Permissions)
                .FirstOrDefaultAsync(t => t.Code == code);

            if (role == null)
            {
                role = new Role
                {
                    Code = code,
                    Name = name
                };

                db.Roles.Add(role);
            }

            db.RolePermissions.AddRange(permissions.Where(t => !role.Permissions.Any(n => n.Value == t))
                .Select(t => new RolePermission
                {
                    Role = role,
                    Value = t
                }).ToArray()
            );
        }
    }
}
