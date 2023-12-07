using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Mappers;
using Izm.Rumis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public sealed class SupervisorService : ISupervisorService
    {
        private readonly IAppDbContext db;

        public SupervisorService(IAppDbContext db)
        {
            this.db = db;
        }

        /// <inheritdoc/>
        public async Task<int> CreateAsync(SupervisorCreateDto item, CancellationToken cancellationToken = default)
        {
            var entity = new Supervisor();

            await db.Supervisors.AddAsync(SupervisorMapper.Map(item, entity), cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await db.Supervisors.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            db.Supervisors.Remove(entity);

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public SetQuery<Supervisor> Get()
        {
            var query = db.Supervisors.AsNoTracking();

            return new SetQuery<Supervisor>(query);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(int id, SupervisorUpdateDto item, CancellationToken cancellationToken = default)
        {
            var entity = await db.Supervisors.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            SupervisorMapper.Map(item, entity);

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
