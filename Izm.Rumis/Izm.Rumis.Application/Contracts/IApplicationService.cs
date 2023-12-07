using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Models.Application;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IApplicationService
    {
        /// <summary>
        /// Check application social statuses.
        /// </summary>
        /// <param name="id">Application ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task<Domain.Entities.Application> CheckApplicationSocialStatusAsync(Guid id, CancellationToken cancellationToken = default);
        /// Create an application.
        /// </summary>
        /// <param name="item">Application creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created application ID and number.</returns>
        Task<ApplicationCreateResult> CreateAsync(ApplicationCreateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Change application status to declined.
        /// </summary>
        /// <param name="item">Application decline data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task DeclineAsync(ApplicationDeclineDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Change application status to deleted.
        /// </summary>
        /// <param name="applicationIds>Application delete ids.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task DeleteAsync(IEnumerable<Guid> applicationIds, CancellationToken cancellationToken = default);
        /// <summary>
        /// Query applications.
        /// </summary>
        /// <returns>Applications query wrapped in <see cref="SetQuery{T}"/>.</returns>
        SetQuery<Domain.Entities.Application> Get();
        /// <summary>
        /// Query application duplicates.
        /// </summary>
        /// <returns>Applications query wrapped in <see cref="SetQuery{T}"/>.</returns>
        SetQuery<Domain.Entities.Application> GetApplicationDuplicates(ApplicationCheckDuplicateDto item);
        /// <summary>
        /// Change application status to postponed.
        /// </summary>
        /// <param name="id">Application ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task PostponeAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update an application.
        /// </summary>
        /// <param name="id">Application ID.</param>
        /// <param name="item">Application update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task UpdateAsync(Guid id, ApplicationUpdateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Change application status to withdrawn.
        /// </summary>
        /// <param name="id">Application ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task WithdrawAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update or create application contact person data.
        /// </summary>
        /// <param name="id">Application ID.</param>
        /// <param name="item">Application contanct person update/create data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ChangeSubmitterContactAsync(Guid id, ApplicationContactInformationUpdateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update or create applications contact person data.
        /// </summary>
        /// <param name="ids">Application IDs.</param>
        /// <param name="item">Applications contact person update/create data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ChangeSubmittersContactAsync(IEnumerable<Guid> ids, ApplicationsContactInformationUpdateDto item, CancellationToken cancellationToken = default);
    }
}
