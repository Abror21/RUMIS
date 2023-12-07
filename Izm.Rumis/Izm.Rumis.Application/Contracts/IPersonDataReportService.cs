using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IPersonDataReportService
    {
        /// <summary>
        /// Generate a person data report.
        /// </summary>
        /// <param name="item">Person data report request data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>GDPR audit entries wrapped in <see cref="SetQuery{T}"/>.</returns>
        Task<SetQuery<GdprAudit>> GenerateAsync(PersonDataReportGenerateDto item, CancellationToken cancellationToken = default);
    }
}
