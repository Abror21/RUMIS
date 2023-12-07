using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Sessions
{
    public interface ISessionService
    {
        /// <summary>
        /// Create a session.
        /// </summary>
        /// <param name="id">Session ID.</param>
        /// <param name="created">Session creation time.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task CreateAsync(Guid id, DateTime? created = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Delete a session.
        /// </summary>
        /// <param name="id">Session ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Delete multiple sessions.
        /// </summary>
        /// <param name="ids">IDs of sessions to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
        /// <summary>
        /// Query sessions.
        /// </summary>
        /// <returns><see cref="IQueryable{T}"/> of sessions wrapped in <see cref="SetQuery{T}"/>.</returns>
        SetQuery<Session> Get();
    }

    public sealed class SessionService : ISessionService
    {
        private readonly ISessionDbContext db;

        public SessionService(ISessionDbContext db)
        {
            this.db = db;
        }

        /// <inheritdoc/>
        public async Task CreateAsync(Guid id, DateTime? created = null, CancellationToken cancellationToken = default)
        {
            db.Sessions.Add(new Session
            {
                Id = id,
                Created = created ?? DateTime.UtcNow
            });

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.Sessions.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            db.Sessions.Remove(entity);

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var entities = await db.Sessions
                .Where(t => ids.Contains(t.Id))
                .ToArrayAsync(cancellationToken);

            if (!entities.Any())
                return;

            db.Sessions.RemoveRange(entities);

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public SetQuery<Session> Get()
        {
            return new SetQuery<Session>(db.Sessions.AsNoTracking());
        }
    }
}
