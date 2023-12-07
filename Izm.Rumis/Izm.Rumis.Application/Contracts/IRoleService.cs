using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IRoleService
    {
        /// <summary>
        /// Create a role.
        /// </summary>
        /// <param name="item">Role creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created role ID.</returns>
        Task<int> CreateAsync(RoleEditDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Delete a role.
        /// </summary>
        /// <param name="id">Role ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Query roles.
        /// </summary>
        /// <returns>Roles query wrapped in <see cref="SetQuery{T}"/>.</returns>
        SetQuery<Role> Get();
        /// <summary>
        /// Update a role.
        /// </summary>
        /// <param name="id">Role ID.</param>
        /// <param name="item">Role update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task UpdateAsync(int id, RoleEditDto item, CancellationToken cancellationToken = default);
    }
}
