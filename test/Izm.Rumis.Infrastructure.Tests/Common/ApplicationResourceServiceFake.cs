using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal sealed class ApplicationResourceServiceFake : IApplicationResourceService
    {
        public IQueryable<ApplicationResource> ApplicationResources { get; set; } = new List<ApplicationResource>().AsQueryable();
        public ApplicationResourceCreateDto CreateWithDraftStatusAsyncCalledWith { get; set; } = null;
        public ApplicationResourceCreateDto CreateWithPreparedStatusAsyncCalledWith { get; set; } = null;
        public ApplicationResourceChangeStatusDto ChangeStatusToLostAsyncCalledWithDto { get; set; } = null;
        public ApplicationResourceChangeStatusDto ChangeStatusToStolenAsyncCalledWithDto { get; set; } = null;
        public ApplicationResourceReturnEditDto ReturnAsyncCalledWithDto { get; set; } = null;
        public ApplicationResourceCancelDto CancelAsyncCalledWithDto { get; set; } = null;
        public ApplicationResourceUpdateDto UpdateAsyncCalledWithDto { get; set; } = null;
        public ApplicationResourceReturnDeadlineDto SetReturnDeadlineAsyncCalledWithDto { get; set; } = null;

        public string ExploitationRules { get; set; }
        public FileDto ExploitationRulesPdf { get; set; }
        public string Pna { get; set; }
        public FileDto PnaPdf { get; set; }
        public Guid ChangeStatusToPreparedAsyncCalledWith { get; set; }
        public Guid SignAsyncCalledWith { get; set; }
        public Guid CancelAsyncCalledWithId { get; set; }
        public Guid ChangeStatusToLostAsyncCalledWithId { get; set; }
        public Guid ChangeStatusToStolenAsyncCalledWithId { get; set; }
        public Guid ReturnAsyncCalledWithId { get; set; }
        public Guid GetExploitationRulesAsyncCalledWith { get; set; }
        public Guid GetExploitationRulesPdfAsyncCalledWith { get; set; }
        public Guid UpdateAsyncCalledWithId { get; set; }
        public Guid GetPnaAsyncCalledWith { get; set; }
        public Guid GetPnaPdfAsyncCalledWith { get; set; }

        public Task ChangeStatusToLostAsync(Guid id, ApplicationResourceChangeStatusDto item, CancellationToken cancellationToken = default)
        {
            ChangeStatusToLostAsyncCalledWithId = id;

            ChangeStatusToLostAsyncCalledWithDto = item;

            return Task.CompletedTask;
        }

        public Task ChangeStatusToPreparedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ChangeStatusToPreparedAsyncCalledWith = id;

            return Task.CompletedTask;
        }

        public Task ChangeStatusToStolenAsync(Guid id, ApplicationResourceChangeStatusDto item, CancellationToken cancellationToken = default)
        {
            ChangeStatusToStolenAsyncCalledWithId = id;

            ChangeStatusToStolenAsyncCalledWithDto = item;

            return Task.CompletedTask;
        }

        public Task<Guid> CreateWithDraftStatusAsync(ApplicationResourceCreateDto item, CancellationToken cancellationToken = default)
        {
            CreateWithDraftStatusAsyncCalledWith = item;

            return Task.FromResult(Guid.NewGuid());
        }

        public Task<Guid> CreateWithPreparedStatusAsync(ApplicationResourceCreateDto item, CancellationToken cancellationToken = default)
        {
            CreateWithPreparedStatusAsyncCalledWith = item;

            return Task.FromResult(Guid.NewGuid());
        }

        public SetQuery<ApplicationResource> Get()
        {
            return new SetQuery<ApplicationResource>(ApplicationResources);
        }

        public Task<string> GetExploitationRulesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            GetExploitationRulesAsyncCalledWith = id;

            return Task.FromResult(ExploitationRules);
        }

        public Task<FileDto> GetExploitationRulesPdfAsync(Guid id, CancellationToken cancellationToken = default)
        {
            GetExploitationRulesPdfAsyncCalledWith = id;

            return Task.FromResult(ExploitationRulesPdf);
        }

        public Task<string> GetPnaAsync(Guid applicationResourceId, CancellationToken cancellationToken = default)
        {
            GetPnaAsyncCalledWith = applicationResourceId;

            return Task.FromResult(Pna);
        }

        public Task<FileDto> GetPnaPdfAsync(Guid applicationResourceId, CancellationToken cancellationToken = default)
        {
            GetPnaPdfAsyncCalledWith = applicationResourceId;

            return Task.FromResult(PnaPdf);
        }

        public Task UpdateAsync(Guid id, ApplicationResourceUpdateDto item, CancellationToken cancellationToken = default)
        {
            UpdateAsyncCalledWithId = id;

            UpdateAsyncCalledWithDto = item;

            return Task.CompletedTask;
        }

        public Task SetReturnDeadlineAsync(ApplicationResourceReturnDeadlineDto item, CancellationToken cancellationToken = default)
        {
            SetReturnDeadlineAsyncCalledWithDto = item;

            return Task.CompletedTask;
        }

        public Task ReturnAsync(Guid id, ApplicationResourceReturnEditDto item, CancellationToken cancellationToken = default)
        {
            ReturnAsyncCalledWithId = id;

            ReturnAsyncCalledWithDto = item;

            return Task.CompletedTask;
        }

        public Task SignAsync(Guid id, CancellationToken cancellationToken = default)
        {
            SignAsyncCalledWith = id;

            return Task.CompletedTask;
        }

        public Task CancelAsync(Guid id, ApplicationResourceCancelDto item, CancellationToken cancellationToken = default)
        {
            CancelAsyncCalledWithId = id;

            CancelAsyncCalledWithDto = item;

            return Task.CompletedTask;
        }
    }
}
