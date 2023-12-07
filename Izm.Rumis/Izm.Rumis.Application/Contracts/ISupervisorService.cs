using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface ISupervisorService
    {
        /// <summary>
        /// Create a supervisor.
        /// </summary>
        /// <param name="item">Supervisor creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created supervisor ID.</returns>
        Task<int> CreateAsync(SupervisorCreateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Delete a supervisor.
        /// </summary>
        /// <param name="id">Supervisor ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Query supervisors.
        /// </summary>
        /// <returns>Supervisors query wrapped in <see cref="SetQuery{T}"/>.</returns>
        SetQuery<Supervisor> Get();
        /// <summary>
        /// Update a supervisor.
        /// </summary>
        /// <param name="id">Supervisor ID.</param>
        /// <param name="item">Supervisor update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task UpdateAsync(int id, SupervisorUpdateDto item, CancellationToken cancellationToken = default);
    }
}
