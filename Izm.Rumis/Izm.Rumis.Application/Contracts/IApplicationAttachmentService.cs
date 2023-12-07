using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IApplicationAttachmentService
    {
        /// <summary>
        /// Get application attachments.
        /// </summary>
        /// <returns></returns>
        SetQuery<ApplicationAttachment> Get();
        /// <summary>
        /// Create an application attachment.
        /// </summary>
        /// <param name="applicationId">Application ID</param>
        /// <param name="item">Application attachment data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<Guid> CreateAsync(ApplicationAttachmentCreateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update an application attachment.
        /// </summary>
        /// <param name="id">Application attachment ID</param>
        /// <param name="item">Application attachment data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task UpdateAsync(Guid id, ApplicationAttachmentUpdateDto item, CancellationToken cancellationToken = default);
    }
}
