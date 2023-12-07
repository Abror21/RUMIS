using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IApplicationResourceService
    {
        /// <summary>
        /// Cancel application resource.
        /// </summary>
        /// <param name="id">Application for certain resource ID.</param>
        /// <param name="item">Cancellation request data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task CancelAsync(Guid id, ApplicationResourceCancelDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Change an application for certain resource status from issued to lost.
        /// </summary>
        /// <param name="id">Application resource ID.</param>
        /// <param name="item">Application resource update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ChangeStatusToLostAsync(Guid id, ApplicationResourceChangeStatusDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Change an application for certain resource status from draft to prepared.
        /// </summary>
        /// <param name="id">Application resource ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ChangeStatusToPreparedAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Change an application for certain resource status from issued to stolen.
        /// </summary>
        /// <param name="id">Application resource ID.</param>
        /// <param name="item">Application resource update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ChangeStatusToStolenAsync(Guid id, ApplicationResourceChangeStatusDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Create an application for certain resource with draft status.
        /// </summary>
        /// <param name="item">Application resource creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created application resource ID.</returns>
        Task<Guid> CreateWithDraftStatusAsync(ApplicationResourceCreateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Create an application for certain resource with prepared status.
        /// </summary>
        /// <param name="item">Application resource creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created application resource ID.</returns>
        Task<Guid> CreateWithPreparedStatusAsync(ApplicationResourceCreateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Query application resources.
        /// </summary>
        /// <returns>Application resources query wrapped in <see cref="SetQuery{T}"/>.</returns>
        SetQuery<ApplicationResource> Get();
        /// <summary>
        /// Get application resource exploitation rule html string.
        /// </summary>
        /// <param name="id">Application resource ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Exploitation rule html string.</returns>
        Task<string> GetExploitationRulesAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get application resource exploitation rule pdf.
        /// </summary>
        /// <param name="id">Application resource ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Exploitation rule pdf.</returns>
        Task<FileDto> GetExploitationRulesPdfAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get application resource PNA html string.
        /// </summary>
        /// <param name="id">Application resource ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>PNA html string.</returns>
        Task<string> GetPnaAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get application resource PNA pdf.
        /// </summary>
        /// <param name="id">Application resource ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>PNA pdf.</returns>
        Task<FileDto> GetPnaPdfAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update an application resource.
        /// </summary>
        /// <param name="id">Application resource ID.</param>
        /// <param name="item">Application resource update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task UpdateAsync(Guid id, ApplicationResourceUpdateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update an application for certain resource during return operation.
        /// </summary>
        /// <param name="id">Application resource ID.</param>
        /// <param name="item">Application resource update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ReturnAsync(Guid id, ApplicationResourceReturnEditDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update return operation deadline.
        /// </summary>
        /// <param name="item">Application resource update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task SetReturnDeadlineAsync(ApplicationResourceReturnDeadlineDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Sign application resource.
        /// </summary>
        /// <param name="id">Application resource ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task SignAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
