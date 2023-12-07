using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Models.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    public sealed class ApplicationServiceFake : IApplicationService
    {
        public IQueryable<Domain.Entities.Application> Applications { get; set; } = new TestAsyncEnumerable<Domain.Entities.Application>(new List<Domain.Entities.Application>());

        public ApplicationCheckDuplicateDto GetApplicationDuplicatesCalledWith { get; set; } = null;
        public ChangeSubmitterContactCalledWithApp ChangeSubmitterContactCalledWith { get; set; } = null;
        public IEnumerable<Guid> ChangeSubmitterContactAsyncCalledWithIds { get; set; } = null;
        public ApplicationsContactInformationUpdateDto ChangeSubmitterContactAsyncCalledWithDto { get; set; } = null;

        public Task ChangeSubmitterContactAsync(Guid id, ApplicationContactInformationUpdateDto item, CancellationToken cancellationToken = default)
        {
            ChangeSubmitterContactCalledWith = new ChangeSubmitterContactCalledWithApp(id, item);

            return Task.CompletedTask;
        }

        public Task ChangeSubmittersContactAsync(IEnumerable<Guid> ids, ApplicationsContactInformationUpdateDto item, CancellationToken cancellationToken = default)
        {
            ChangeSubmitterContactAsyncCalledWithIds = ids;

            ChangeSubmitterContactAsyncCalledWithDto = item;

            return Task.CompletedTask;
        }

        public Task<Domain.Entities.Application> CheckApplicationSocialStatusAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Domain.Entities.Application());
        }

        public Task<ApplicationCreateResult> CreateAsync(ApplicationCreateDto item, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ApplicationCreateResult(Guid.NewGuid(), string.Empty));
        }

        public SetQuery<Domain.Entities.Application> Get()
        {
            return new SetQuery<Domain.Entities.Application>(Applications);
        }

        public SetQuery<Domain.Entities.Application> GetApplicationDuplicates(ApplicationCheckDuplicateDto item)
        {
            GetApplicationDuplicatesCalledWith = item;

            return new SetQuery<Domain.Entities.Application>(Applications);
        }

        public Task UpdateAsync(Guid id, ApplicationUpdateDto item, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task WithdrawAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task PostponeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeclineAsync(ApplicationDeclineDto item, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(IEnumerable<Guid> applicationIds, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public record ChangeSubmitterContactCalledWithApp(Guid Id, ApplicationContactInformationUpdateDto Item);
}
