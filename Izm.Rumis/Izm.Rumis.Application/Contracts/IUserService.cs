using Izm.Rumis.Application.Common;
using Izm.Rumis.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IUserService
    {
        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Query users that have a person association.
        /// </summary>
        /// <returns>Users query wrapped in <see cref="SetQuery{T}"/>.</returns>
        SetQuery<User> GetPersons();
    }
}
