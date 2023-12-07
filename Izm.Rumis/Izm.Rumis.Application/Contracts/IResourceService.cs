using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IResourceService
    {
        /// <summary>
        /// Query resources.
        /// </summary>
        /// <returns>Resources query wrapped in <see cref="SetQuery{T}"/>.</returns>
        SetQuery<Resource> Get();
        /// <summary>
        /// Create a resource.
        /// </summary>
        /// <param name="item">Resource creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created resource ID.</returns>
        Task<Guid> CreateAsync(ResourceCreateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update a resource.
        /// </summary>
        /// <param name="id">Resource ID.</param>
        /// <param name="item">Resource update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task UpdateAsync(Guid id, ResourceUpdateDto item, CancellationToken cancellationToken = default);
    }
}
