using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal class ApplicationAttachmentServiceFake : IApplicationAttachmentService
    {
        public IQueryable<ApplicationAttachment> ApplicationAttachments { get; set; } = new TestAsyncEnumerable<ApplicationAttachment>(new List<ApplicationAttachment>());

        public Guid CreateResult { get; set; } = new Guid();

        public Task<Guid> CreateAsync(ApplicationAttachmentCreateDto item, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResult);
        }

        public SetQuery<ApplicationAttachment> Get()
        {
            return new SetQuery<ApplicationAttachment>(ApplicationAttachments);
        }

        public Task UpdateAsync(Guid id, ApplicationAttachmentUpdateDto item, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
