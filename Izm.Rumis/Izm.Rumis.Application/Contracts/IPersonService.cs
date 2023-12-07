using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IPersonService
    {
        /// <summary>
        /// Create a person.
        /// </summary>
        /// <param name="item">Person data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created person ID and related user ID.</returns>
        Task<PersonCreateResponseDto> CreateAsync(PersonCreateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Ensure user for person.
        /// </summary>
        /// <param name="id">Private ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>User ID</returns>
        Task<Guid?> EnsureUserAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Query persons.
        /// </summary>
        /// <returns>Person query wrapped in <see cref="SetQuery{T}"/>.</returns>
        SetQuery<Person> Get();
        /// <summary>
        /// Update a person.
        /// </summary>
        /// <param name="privatePersonalIdentifier">Person Private personal identifier.</param>
        /// <param name="item">Person update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task UpdateAsync(Guid id, PersonUpdateDto item, CancellationToken cancellationToken = default);
    }
}
