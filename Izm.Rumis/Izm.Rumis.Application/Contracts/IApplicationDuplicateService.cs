using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IApplicationDuplicateService
    {
        /// <summary>
        /// Check applications duplicates.
        /// </summary>
        /// <param name="applicationIds">Application IDs.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task CheckApplicationsDuplicatesAsync(IEnumerable<Guid> applicationIds, CancellationToken cancellationToken = default);
    }
}
