using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.EServices.Dtos;
using Izm.Rumis.Infrastructure.EServices.Models;
using Izm.Rumis.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal sealed class EServicesServiceFake : IEServicesService
    {
        public IEnumerable<EServicesRelatedPersonResponseDto> EServicesRelatedPersons { get; set; } = new TestAsyncEnumerable<EServicesRelatedPersonResponseDto>(new List<EServicesRelatedPersonResponseDto>());
        public IQueryable<Domain.Entities.Application> Applications { get; set; } = new TestAsyncEnumerable<Domain.Entities.Application>(new List<Domain.Entities.Application>());
        public IQueryable<Classifier> Classifiers { get; set; } = new TestAsyncEnumerable<Classifier>(new List<Classifier>());
        public IQueryable<EducationalInstitution> EducationalInstitutions { get; set; } = new TestAsyncEnumerable<EducationalInstitution>(new List<EducationalInstitution>());
        public IQueryable<DocumentTemplate> DocumentTemplates { get; set; } = new TestAsyncEnumerable<DocumentTemplate>(new List<DocumentTemplate>());
        public IEnumerable<EServiceEmployeeResponseDto> Employee { get; set; } = new TestAsyncEnumerable<EServiceEmployeeResponseDto>(new List<EServiceEmployeeResponseDto>());
        public string ApplicationResourcePna { get; set; }
        public FileDto ApplicationResourcePnaPdf { get; set; }
        public string ApplicationResourceExploitationRules { get; set; }
        public FileDto ApplicationResourceExploitationRulesPdf { get; set; }
        public string ApplicationResourceDocumentSample { get; set; }
        public string DocumentTemplateHtml { get; set; }
        public FileDto DocumentTemplatePdf { get; set; }

        public EServiceApplicationCheckDuplicateDto GetApplicationDuplicatesAsyncCalledWith { get; set; } = null;
        public EServiceApplicationCreateDto CreateApplicationAsyncCalledWith { get; set; } = null;
        public ChangeSubmitterContactCalledWith ChangeSubmitterContactCalledWith { get; set; } = null;
        public EServiceChangeStatusDto ChangeStatusToLostAsyncCalledWithDto { get; set; } = null;
        public EServiceChangeStatusDto ChangeStatusToStolenAsyncCalledWithDto { get; set; } = null;
        public EServiceApplicationsChangeContactPersonDto ChangeSubmittersContactAsyncCalledWithDto { get; set; } = null;
        public EServiceDocumentTemplatesDto GetDocumentTemplatesCalledWith { get; set; } = null;
        public Guid ChangeStatusToLostAsyncCalledWithId { get; set; }
        public Guid ChangeStatusToStolenAsyncCalledWithId { get; set; }
        public IEnumerable<Guid> ChangeSubmittersContactAsyncCalledWithIds { get; set; }
        public Guid GetApplicationResourcePnaAsyncCalledWith { get; set; }
        public Guid GetApplicationResourcePnaPdfAsyncCalledWith { get; set; }
        public Guid GetApplicationResourceExploitationRulesAsyncCalledWith { get; set; }
        public Guid GetApplicationResourceExploitationRulesPdfAsyncCalledWith { get; set; }
        public string GetDocumentSampleAsyncCalledWith { get; set; } = null;
        public Guid SignApplicationResourceCalledWith { get; set; }
        public int GetDocumentTemplateAsyncCalledWith { get; set; }
        public int GetDocumentTemplatePdfAsyncCalledWith { get; set; }

        public Task ChangeStatusToLostAsync(Guid id, EServiceChangeStatusDto item, CancellationToken cancellationToken = default)
        {
            ChangeStatusToLostAsyncCalledWithId = id;

            ChangeStatusToLostAsyncCalledWithDto = item;

            return Task.CompletedTask;
        }

        public Task ChangeStatusToStolenAsync(Guid id, EServiceChangeStatusDto item, CancellationToken cancellationToken = default)
        {
            ChangeStatusToStolenAsyncCalledWithId = id;

            ChangeStatusToStolenAsyncCalledWithDto = item;

            return Task.CompletedTask;
        }

        public Task ChangeSubmitterContactAsync(Guid id, EServiceApplicationChangeContactPersonDto item, CancellationToken cancellationToken = default)
        {
            ChangeSubmitterContactCalledWith = new ChangeSubmitterContactCalledWith(id, item);

            return Task.CompletedTask;
        }

        public Task ChangeSubmittersContactAsync(IEnumerable<Guid> ids, EServiceApplicationsChangeContactPersonDto item, CancellationToken cancellationToken = default)
        {
            ChangeSubmittersContactAsyncCalledWithIds = ids;

            ChangeSubmittersContactAsyncCalledWithDto = item;

            return Task.CompletedTask;
        }

        public Task<EServiceApplicationCreateResult> CreateApplicationAsync(EServiceApplicationCreateDto item, CancellationToken cancellationToken = default)
        {
            CreateApplicationAsyncCalledWith = item;

            return Task.FromResult(new EServiceApplicationCreateResult(Guid.NewGuid(), string.Empty));
        }

        public SetQuery<Domain.Entities.Application> GetApplications(IEnumerable<string> privatePersonalIdentifiers)
        {
            return new SetQuery<Domain.Entities.Application>(Applications);
        }

        public Task<SetQuery<Domain.Entities.Application>> GetApplicationDuplicatesAsync(EServiceApplicationCheckDuplicateDto item, CancellationToken cancellationToken = default)
        {
            GetApplicationDuplicatesAsyncCalledWith = item;

            return Task.FromResult(new SetQuery<Domain.Entities.Application>(Applications));
        }

        public Task<string> GetApplicationResourcePnaAsync(Guid applicationResourceId, CancellationToken cancellationToken = default)
        {
            GetApplicationResourcePnaAsyncCalledWith = applicationResourceId;

            return Task.FromResult(ApplicationResourcePna);
        }

        public Task<FileDto> GetApplicationResourcePnaPdfAsync(Guid applicationResourceId, CancellationToken cancellationToken = default)
        {
            GetApplicationResourcePnaPdfAsyncCalledWith = applicationResourceId;

            return Task.FromResult(ApplicationResourcePnaPdf);
        }

        public Task<string> GetApplicationResourceExploitationRulesAsync(Guid applicationResourceId, CancellationToken cancellationToken = default)
        {
            GetApplicationResourceExploitationRulesAsyncCalledWith = applicationResourceId;

            return Task.FromResult(ApplicationResourceExploitationRules);
        }

        public Task<FileDto> GetApplicationResourceExploitationRulesPdfAsync(Guid applicationResourceId, CancellationToken cancellationToken = default)
        {
            GetApplicationResourceExploitationRulesPdfAsyncCalledWith = applicationResourceId;

            return Task.FromResult(ApplicationResourceExploitationRulesPdf);
        }

        public Task<string> GetDocumentSampleAsync(int eduInstId, string documentTemplateCode, CancellationToken cancellationToken = default)
        {
            GetDocumentSampleAsyncCalledWith = documentTemplateCode;

            return Task.FromResult(ApplicationResourceDocumentSample);
        }

        public Task<IEnumerable<EServicesRelatedPersonResponseDto>> GetAsEducatee(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EServicesRelatedPersons);
        }

        public Task<IEnumerable<EServicesRelatedPersonResponseDto>> GetAsParentOrGuardian(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EServicesRelatedPersons);
        }
        public Task<IEnumerable<EServiceEmployeeResponseDto>> GetEmployeesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Employee);
        }

        public SetQuery<Classifier> GetClassifiers(IEnumerable<string> types)
        {
            return new SetQuery<Classifier>(Classifiers);
        }

        public SetQuery<EducationalInstitution> GetEducationalInstitutions(IEnumerable<string> regNrs)
        {
            return new SetQuery<EducationalInstitution>(EducationalInstitutions);
        }

        public Task WithdrawApplicationAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SignApplicationResourceAsync(Guid id, CancellationToken cancellationToken = default)
        {
            SignApplicationResourceCalledWith = id;

            return Task.CompletedTask;
        }

        public SetQuery<DocumentTemplate> GetDocumentTemplates(EServiceDocumentTemplatesDto dto)
        {
            GetDocumentTemplatesCalledWith = dto;

            return new SetQuery<DocumentTemplate>(DocumentTemplates);
        }

        public Task<string> GetDocumentTemplateAsync(int id, CancellationToken cancellationToken = default)
        {
            GetDocumentTemplateAsyncCalledWith = id;

            return Task.FromResult(DocumentTemplateHtml);
        }

        public Task<FileDto> GetDocumentTemplatePdfAsync(int id, CancellationToken cancellationToken = default)
        {
            GetDocumentTemplatePdfAsyncCalledWith = id;

            return Task.FromResult(DocumentTemplatePdf);
        }
    }

    public record ChangeSubmitterContactCalledWith(Guid Id, EServiceApplicationChangeContactPersonDto Item);
}
