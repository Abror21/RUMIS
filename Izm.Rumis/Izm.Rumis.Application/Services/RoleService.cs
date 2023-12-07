using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Mappers;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public sealed class RoleService : IRoleService
    {
        private readonly IAppDbContext db;

        public RoleService(IAppDbContext db)
        {
            this.db = db;
        }

        /// <inheritdoc/>
        public async Task<int> CreateAsync(RoleEditDto item, CancellationToken cancellationToken = default)
        {
            var entity = new Role();

            RoleMapper.Map(item, entity);

            entity.Permissions = item.Permissions
                .Intersect(Permission.All)
                .Select(t => new RolePermission
                {
                    Value = t
                })
                .ToArray();

            db.Roles.Add(entity);

            await db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await db.Roles
                .Include(t => t.Permissions)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            db.RolePermissions.RemoveRange(entity.Permissions);
            db.Roles.Remove(entity);

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public SetQuery<Role> Get()
        {
            return new SetQuery<Role>(db.Roles.AsNoTracking());
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(int id, RoleEditDto item, CancellationToken cancellationToken = default)
        {
            var entity = await db.Roles
                .Include(t => t.Permissions)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            RoleMapper.Map(item, entity);

            var permissions = item.Permissions
                .Intersect(Permission.All)
                .ToArray();

            db.RolePermissions.AddRange(
                permissions
                    .Where(t => !entity.Permissions.Any(n => n.Value == t))
                    .Select(t => new RolePermission
                    {
                        RoleId = entity.Id,
                        Value = t
                    })
                );
            db.RolePermissions.RemoveRange(
                entity.Permissions.Where(t => !permissions.Contains(t.Value))
                );

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
