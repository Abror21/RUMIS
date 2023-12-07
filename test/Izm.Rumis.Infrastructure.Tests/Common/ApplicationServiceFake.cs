using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Models.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal sealed class ApplicationServiceFake : IApplicationService
    {
        public IQueryable<Domain.Entities.Application> Applications { get; set; } = new List<Domain.Entities.Application>().AsQueryable();
        public ApplicationCreateDto CreateAsyncCalledWith { get; set; } = null;
        public ApplicationUpdateDto UpdateAsyncCalledWith { get; set; } = null;
        public ApplicationCheckDuplicateDto GetApplicationDuplicatesCalledWith { get; set; } = null;
        public Domain.Entities.Application CheckApplicationSocialStatusAsyncResult { get; set; } = new Domain.Entities.Application();
        public ChangeSubmitterContactCalledWith ChangeSubmitterContactCalledWith { get; set; } = null;
        public Guid? WithdrawAsyncCalledWith { get; set; } = null;
        public IEnumerable<Guid> ChangeSubmitterContactAsyncCalledWithIds { get; set; } = null;
        public ApplicationsContactInformationUpdateDto ChangeSubmitterContactAsyncCalledWithDto { get; set; } = null;

        public Task ChangeSubmitterContactAsync(Guid id, ApplicationContactInformationUpdateDto model, CancellationToken cancellationToken = default)
        {
            ChangeSubmitterContactCalledWith = new ChangeSubmitterContactCalledWith(id, model);

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
            return Task.FromResult(CheckApplicationSocialStatusAsyncResult);
        }

        public Task<ApplicationCreateResult> CreateAsync(ApplicationCreateDto item, CancellationToken cancellationToken = default)
        {
            CreateAsyncCalledWith = item;

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
            UpdateAsyncCalledWith = item;

            return Task.CompletedTask;
        }

        public Task PostponeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task WithdrawAsync(Guid id, CancellationToken cancellationToken = default)
        {
            WithdrawAsyncCalledWith = id;

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

    public record ChangeSubmitterContactCalledWith(Guid Id, ApplicationContactInformationUpdateDto Item);
}
