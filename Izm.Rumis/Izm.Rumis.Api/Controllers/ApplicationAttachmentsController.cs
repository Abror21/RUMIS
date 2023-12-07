using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Services;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    [PermissionAuthorize(Permission.ApplicationView)]
    public class ApplicationAttachmentsController : ApiController
    {
        private readonly IApplicationAttachmentService service;

        public ApplicationAttachmentsController(IApplicationAttachmentService service)
        {
            this.service = service;
        }

        [HttpGet("{applicationId}")]
        public async Task<ActionResult<IEnumerable<ApplicationAttachmentResponse>>> Get(Guid applicationId, CancellationToken cancellationToken = default)
        {
            var data = await service.Get().Where(t => t.ApplicationId == applicationId)
                .ListAsync(map: ApplicationAttachmentMapper.ProjectListItem(), cancellationToken: cancellationToken);

            return data.ToList();
        }

        [HttpPost]
        [PermissionAuthorize(Permission.ApplicationEdit)]
        public async Task<ActionResult<Guid>> Post([FromForm] ApplicationAttachmentCreateRequest model, CancellationToken cancellationToken = default)
        {
            var dto = ApplicationAttachmentMapper.Map(model, new ApplicationAttachmentCreateDto());

            var result = await service.CreateAsync(dto, cancellationToken);

            return result;
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.ApplicationEdit)]
        public async Task<IActionResult> Put(Guid id, [FromForm] ApplicationAttachmentUpdateRequest model, CancellationToken cancellationToken = default)
        {
            var dto = ApplicationAttachmentMapper.Map(model, new ApplicationAttachmentUpdateDto());

            await service.UpdateAsync(id, dto, cancellationToken);

            return NoContent();
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download([FromServices] IFileManager fileManager, Guid id, CancellationToken cancellationToken = default)
        {
            var attachment = await service.Get().Where(t => t.Id == id)
                .FirstAsync(t => new { t.FileId }, cancellationToken);

            return attachment == null ? NotFound() : await fileManager.Download(attachment.FileId);
        }
    }
}
