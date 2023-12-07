using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IClassifierService
    {
        /// <summary>
        /// Create a classifier.
        /// </summary>
        /// <param name="item">Classifier data</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>New classifier ID</returns>
        Task<Guid> CreateAsync(ClassifierCreateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Delete a classifier.
        /// </summary>
        /// <param name="id">Classifier ID</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get classifiers.
        /// </summary>
        /// <returns></returns>
        SetQuery<Classifier> Get();
        /// <summary>
        /// Update a classifier.
        /// </summary>
        /// <param name="id">Classifier ID</param>
        /// <param name="item">Classifier data</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task UpdateAsync(Guid id, ClassifierUpdateDto item, CancellationToken cancellationToken = default);
    }
}
