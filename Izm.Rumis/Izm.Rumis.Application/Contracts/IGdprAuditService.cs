using Izm.Rumis.Application.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IGdprAuditService
    {
        /// <summary>
        /// Trace person data processing by creating a GDPR audit entry.
        /// </summary>
        /// <param name="item">GDPR audit entry data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task TraceAsync(GdprAuditTraceDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Trace a range of person data processing by creating a GDPR audit entries.
        /// </summary>
        /// <param name="items">Collection of GDPR audit entry data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task TraceRangeAsync(IEnumerable<GdprAuditTraceDto> items, CancellationToken cancellationToken = default);
    }
}
