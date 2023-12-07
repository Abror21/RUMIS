using Izm.Rumis.Application;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using Izm.Rumis.Infrastructure.Enums;
using Izm.Rumis.Infrastructure.EServices;
using Izm.Rumis.Infrastructure.EServices.Dtos;
using Izm.Rumis.Infrastructure.EServices.Models;
using Izm.Rumis.Infrastructure.Viis;
using Izm.Rumis.Infrastructure.Vraa;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Services
{
    public interface IEServicesService
    {
        /// <summary>
        /// Create an application.
        /// </summary>
        /// <param name="item">Application data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created application ID and number.</returns>
        Task<EServiceApplicationCreateResult> CreateApplicationAsync(EServiceApplicationCreateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get applications.
        /// </summary>
        /// <param name="privatePersonalIdentifiers">Private personal identifiers</param>
        /// <returns></returns>
        SetQuery<Domain.Entities.Application> GetApplications(IEnumerable<string> privatePersonalIdentifiers);
        /// <summary>
        /// Get application duplicates.
        /// </summary>
        /// <param name="item">Application data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Applications query wrapped in <see cref="SetQuery{T}"/>.</returns>
        Task<SetQuery<Domain.Entities.Application>> GetApplicationDuplicatesAsync(EServiceApplicationCheckDuplicateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get application resource PNA html string.
        /// </summary>
        /// <param name="applicationResourceId">Application resource ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>PNA html string.</returns>
        Task<string> GetApplicationResourcePnaAsync(Guid applicationResourceId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get application resource PNA pdf.
        /// </summary>
        /// <param name="applicationResourceId">Application resource ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>PNA pdf.</returns>
        Task<FileDto> GetApplicationResourcePnaPdfAsync(Guid applicationResourceId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get application resource exploitation rule html string.
        /// </summary>
        /// <param name="applicationResourceId">Application resource ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Exploitation rule html string.</returns>
        Task<string> GetApplicationResourceExploitationRulesAsync(Guid applicationResourceId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get application resource exploitation rule pdf.
        /// </summary>
        /// <param name="applicationResourceId">Application resource ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Exploitation rule pdf.</returns>
        Task<FileDto> GetApplicationResourceExploitationRulesPdfAsync(Guid applicationResourceId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get document sample html string.
        /// </summary>
        /// <param name="educationalInstitutionId">Educational institution Id.</param>
        /// <param name="documentTemplateCode">Document template code.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>PNA html string.</returns>
        Task<string> GetDocumentSampleAsync(int educationalInstitutionId, string documentTemplateCode, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get students attached to educatee.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task<IEnumerable<EServicesRelatedPersonResponseDto>> GetAsEducatee(CancellationToken cancellationToken = default);
        /// <summary>
        /// Get students attached to parent or guardian.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task<IEnumerable<EServicesRelatedPersonResponseDto>> GetAsParentOrGuardian(CancellationToken cancellationToken = default);
        /// <summary>
        /// Get active positions and related institutions to employee.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task<IEnumerable<EServiceEmployeeResponseDto>> GetEmployeesAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Get classifiers.
        /// </summary>
        /// <param name="types">Classifiers types</param>
        /// <returns></returns>
        SetQuery<Classifier> GetClassifiers(IEnumerable<string> types);
        /// <summary>
        /// Get educational institutions.
        /// </summary>
        /// <param name="regNrs">Registration numbers</param>
        /// <returns></returns>
        SetQuery<EducationalInstitution> GetEducationalInstitutions(IEnumerable<string> regNrs);
        /// <summary>
        /// Change application status to withdrawn.
        /// </summary>
        /// <param name="id">Application ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task WithdrawApplicationAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update or create application contact person data.
        /// </summary>
        /// <param name="id">Application ID.</param>
        /// <param name="item">Application contanct person update/create data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ChangeSubmitterContactAsync(Guid id, EServiceApplicationChangeContactPersonDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update or create applications contact person data.
        /// </summary>
        /// <param name="ids">Application IDs.</param>
        /// <param name="item">Applications contact person update/create data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ChangeSubmittersContactAsync(IEnumerable<Guid> ids, EServiceApplicationsChangeContactPersonDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Change an application for certain resource status from issued to lost.
        /// </summary>
        /// <param name="id">Application for certain resource ID.</param>
        /// <param name="item">Application for certain resource update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ChangeStatusToLostAsync(Guid id, EServiceChangeStatusDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Change an application for certain resource status from issued to stolen.
        /// </summary>
        /// <param name="id">Application for certain resource ID.</param>
        /// <param name="item">Application for certain resource update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ChangeStatusToStolenAsync(Guid id, EServiceChangeStatusDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update an application for certain resource.
        /// </summary>
        /// <param name="id">Application for certain resource ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task SignApplicationResourceAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get document templates.
        /// </summary>
        /// <param name="item">Document template filtering data.</param>
        /// <returns></returns>
        SetQuery<DocumentTemplate> GetDocumentTemplates(EServiceDocumentTemplatesDto item);
        /// <summary>
        /// Get document template html string.
        /// </summary>
        /// <param name="id">Document template ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Document template html string.</returns>
        Task<string> GetDocumentTemplateAsync(int id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get document template PNA pdf.
        /// </summary>
        /// <param name="id">>Document template ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Document template pdf.</returns>
        Task<FileDto> GetDocumentTemplatePdfAsync(int id, CancellationToken cancellationToken = default);

    }

    public class EServicesService : IEServicesService
    {
        private readonly IAppDbContext db;
        private readonly IApplicationService applicationService;
        private readonly IApplicationResourceService applicationResourceService;
        private readonly IAuthorizationService authorizationService;
        private readonly IVraaUser vraaUser;
        private readonly IViisService viisService;
        private readonly IGdprAuditService gdprAuditService;
        private readonly IDocumentTemplateService documentTemplateService;
        private readonly IFileService fileService;

        public EServicesService(
            IAppDbContext db,
            IApplicationService applicationService,
            IAuthorizationService authorizationService,
            IApplicationResourceService applicationResourceService,
            IVraaUser vraaUser,
            IViisService viisService,
            IGdprAuditService gdprAuditService,
            IDocumentTemplateService documentTemplateService,
            IFileService fileService)
        {
            this.db = db;
            this.applicationService = applicationService;
            this.authorizationService = authorizationService;
            this.applicationResourceService = applicationResourceService;
            this.vraaUser = vraaUser;
            this.viisService = viisService;
            this.gdprAuditService = gdprAuditService;
            this.documentTemplateService = documentTemplateService;
            this.fileService = fileService;
        }


        /// <inheritdoc />
        public async Task ChangeSubmitterContactAsync(Guid id, EServiceApplicationChangeContactPersonDto item, CancellationToken cancellationToken = default)
        {
            var dto = new ApplicationContactInformationUpdateDto
            {
                Person = new PersonData
                {
                    ContactInformation = item.ContactInformationData.Select(t => new PersonData.ContactData
                    {
                        TypeId = t.TypeId,
                        Value = t.Value
                    }).ToArray(),
                    FirstName = vraaUser.FirstName,
                    LastName = vraaUser.LastName,
                    PrivatePersonalIdentifier = vraaUser.PrivatePersonalIdentifier
                }
            };

            await applicationService.ChangeSubmitterContactAsync(id, dto, cancellationToken);
        }

        /// <inheritdoc />
        public async Task ChangeSubmittersContactAsync(IEnumerable<Guid> ids, EServiceApplicationsChangeContactPersonDto item, CancellationToken cancellationToken = default)
        {
            var dto = new ApplicationsContactInformationUpdateDto
            {
                Person = new PersonData
                {
                    ContactInformation = item.ContactInformationData.Select(t => new PersonData.ContactData
                    {
                        TypeId = t.TypeId,
                        Value = t.Value
                    }).ToArray(),
                    FirstName = vraaUser.FirstName,
                    LastName = vraaUser.LastName,
                    PrivatePersonalIdentifier = vraaUser.PrivatePersonalIdentifier
                }
            };

            await applicationService.ChangeSubmittersContactAsync(ids, dto, cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task ChangeStatusToLostAsync(Guid id, EServiceChangeStatusDto item, CancellationToken cancellationToken = default)
        {
            await applicationResourceService.ChangeStatusToLostAsync(id, EServiceMapper.Map(item, new ApplicationResourceChangeStatusDto()), cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task ChangeStatusToStolenAsync(Guid id, EServiceChangeStatusDto item, CancellationToken cancellationToken = default)
        {
            await applicationResourceService.ChangeStatusToStolenAsync(id, EServiceMapper.Map(item, new ApplicationResourceChangeStatusDto()), cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<EServiceApplicationCreateResult> CreateApplicationAsync(EServiceApplicationCreateDto item, CancellationToken cancellationToken = default)
        {
            var dto = new ApplicationCreateDto
            {
                SubmitterPerson = new PersonData
                {
                    ContactInformation = item.SubmitterContactData.Select(t => new PersonData.ContactData
                    {
                        TypeId = t.TypeId,
                        Value = t.Value
                    }),
                    FirstName = vraaUser.FirstName,
                    LastName = vraaUser.LastName,
                    PrivatePersonalIdentifier = vraaUser.PrivatePersonalIdentifier
                }
            };

            return EServiceApplicationCreateResult.From(
                await applicationService.CreateAsync(EServiceMapper.Map(item, dto), cancellationToken)
                );
        }

        /// <inheritdoc />
        public SetQuery<Domain.Entities.Application> GetApplications(IEnumerable<string> privatePersonalIdentifiers)
        {
            var query = db.Applications.AsNoTracking();

            query = query.Where(t =>
                        t.ApplicationStatus.Code != ApplicationStatus.Deleted
                        && t.ApplicationStatus.Code != ApplicationStatus.Withdrawn
                        && t.ResourceTargetPerson.Persons.Any(p => privatePersonalIdentifiers.Contains(p.PrivatePersonalIdentifier)));

            return new SetQuery<Domain.Entities.Application>(query);
        }

        /// <inheritdoc/>
        public async Task<SetQuery<Domain.Entities.Application>> GetApplicationDuplicatesAsync(EServiceApplicationCheckDuplicateDto item, CancellationToken cancellationToken = default)
        {
            await authorizationService.AuthorizeAsync(vraaUser.PrivatePersonalIdentifier, item.PrivatePersonalIdentifier, cancellationToken);

            return applicationService.GetApplicationDuplicates(EServiceMapper.Map(item, new ApplicationCheckDuplicateDto()));
        }

        /// <inheritdoc/>
        public async Task<string> GetApplicationResourcePnaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            await authorizationService.AuthorizeAsync(
                vraaUser.PrivatePersonalIdentifier,
                entity.Application.ResourceTargetPerson.GetPerson().PrivatePersonalIdentifier,
                cancellationToken);

            return await applicationResourceService.GetPnaAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<FileDto> GetApplicationResourcePnaPdfAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            await authorizationService.AuthorizeAsync(
                vraaUser.PrivatePersonalIdentifier,
                entity.Application.ResourceTargetPerson.GetPerson().PrivatePersonalIdentifier,
                cancellationToken);

            return await applicationResourceService.GetPnaPdfAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<string> GetApplicationResourceExploitationRulesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            await authorizationService.AuthorizeAsync(
                vraaUser.PrivatePersonalIdentifier,
                entity.Application.ResourceTargetPerson.GetPerson().PrivatePersonalIdentifier,
                cancellationToken);

            return await applicationResourceService.GetExploitationRulesAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<FileDto> GetApplicationResourceExploitationRulesPdfAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.ApplicationResources.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            await authorizationService.AuthorizeAsync(
                vraaUser.PrivatePersonalIdentifier,
                entity.Application.ResourceTargetPerson.GetPerson().PrivatePersonalIdentifier,
                cancellationToken);

            return await applicationResourceService.GetExploitationRulesPdfAsync(id, cancellationToken);
        }

        public async Task<string> GetDocumentSampleAsync(int educationalInstitutionId, string docTempCode, CancellationToken cancellationToken = default)
        {
            var entity = await db.EducationalInstitutions.FindAsync(new object[] { educationalInstitutionId }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            var documentTemplateFileId = await documentTemplateService
                .GetByEducationalInstitution(entity.Id)
                .Where(t => t.Code == docTempCode)
                .FirstAsync(t => t.FileId, cancellationToken);

            var template = await fileService.GetAsync(documentTemplateFileId);
            var templateHtml = Encoding.UTF8.GetString(template.Content);

            var values = await db.Classifiers
                .Where(t => t.Type == ClassifierTypes.Placeholder)
                .ToDictionaryAsync(
                    t => t.Code,
                    t => (object)JsonSerializer.Deserialize<PlaceholderPayload>(t.Payload).Value, cancellationToken);

            return HtmlTemplateParser.Parse(templateHtml, values);
        }

        /// <inheritdoc />
        public Task<IEnumerable<EServicesRelatedPersonResponseDto>> GetAsEducatee(CancellationToken cancellationToken = default)
            => GetRelatedPersonData(RequestParamType.Student, cancellationToken);

        /// <inheritdoc />
        public Task<IEnumerable<EServicesRelatedPersonResponseDto>> GetAsParentOrGuardian(CancellationToken cancellationToken = default)
            => GetRelatedPersonData(RequestParamType.Parent, cancellationToken);

        /// <inheritdoc />
        public SetQuery<Classifier> GetClassifiers(IEnumerable<string> types)
        {
            var eServicesTypes = db.Classifiers.Where(t => t.Type == ClassifierTypes.ClassifierType && t.Payload != null && types.Contains(t.Code))
                .Select(t => new { t.Payload, t.Code }).ToList()
                .Where(t => ClassifierPayload.Parse<ClassifierTypePayload>(t.Payload).InEServices == true).Select(t => t.Code);

            var query = db.Classifiers.AsNoTracking();

            query = query.Where(t => eServicesTypes.Contains(t.Type));

            return new SetQuery<Classifier>(query);
        }

        /// <inheritdoc />
        public SetQuery<EducationalInstitution> GetEducationalInstitutions(IEnumerable<string> regNrs)
        {
            var query = db.EducationalInstitutions.AsNoTracking();

            query = query.Where(t => regNrs.Contains(t.Code));

            return new SetQuery<EducationalInstitution>(query);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task SignApplicationResourceAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var resourceTargetPrivatePersonalIdentifier = await db.Applications
                .Where(t => t.ApplicationResources.Any(t => t.Id == id))
                .SelectMany(t => t.ResourceTargetPerson.Persons)
                .OrderByDescending(t => t.ActiveFrom)
                .Select(t => t.PrivatePersonalIdentifier)
                .FirstOrDefaultAsync(cancellationToken);

            if (resourceTargetPrivatePersonalIdentifier == null)
                throw new EntityNotFoundException();

            await authorizationService.AuthorizeAsync(vraaUser.PrivatePersonalIdentifier, resourceTargetPrivatePersonalIdentifier, cancellationToken);

            await applicationResourceService.SignAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task WithdrawApplicationAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var resourceTargetPrivatePersonalIdentifier = await db.Applications
                .Where(t => t.Id == id)
                .SelectMany(t => t.ResourceTargetPerson.Persons)
                .OrderByDescending(t => t.ActiveFrom)
                .Select(t => t.PrivatePersonalIdentifier)
                .FirstOrDefaultAsync(cancellationToken);

            if (resourceTargetPrivatePersonalIdentifier == null)
                throw new EntityNotFoundException();

            await authorizationService.AuthorizeAsync(vraaUser.PrivatePersonalIdentifier, resourceTargetPrivatePersonalIdentifier, cancellationToken);

            await applicationService.WithdrawAsync(id, cancellationToken);
        }

        public SetQuery<DocumentTemplate> GetDocumentTemplates(EServiceDocumentTemplatesDto item)
        {
            return documentTemplateService
                .GetByEducationalInstitution(item.EducationalInstitutionId)
                .Where(t => t.ResourceTypeId == item.ResourceTypeId && t.Code != DocumentType.PNA);
        }

        /// <inheritdoc/>
        public async Task<string> GetDocumentTemplateAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await db.DocumentTemplates.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            var template = await fileService.GetAsync(entity.FileId);

            return Encoding.UTF8.GetString(template.Content);
        }

        /// <inheritdoc/>
        public async Task<FileDto> GetDocumentTemplatePdfAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await db.DocumentTemplates.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            var template = await fileService.GetAsync(entity.FileId);

            return new FileDto
            {
                FileName = $"{entity.Id}.pdf",
                ContentType = MediaTypeNames.Application.Pdf,
                Content = fileService.HtmlToPdf(Encoding.UTF8.GetString(template.Content))
            };
        }

        private async Task<IEnumerable<EServicesRelatedPersonResponseDto>> GetRelatedPersonData(RequestParamType type, CancellationToken cancellationToken = default)
        {
            var data = await viisService.GetStudentsAsync(
                   type: type,
                   privatePersonalIdentifier: vraaUser.PrivatePersonalIdentifier,
                   cancellationToken: cancellationToken);

            var regNrs = data.SelectMany(student => student.Institution.Select(institution => institution.RegNr))
                .Distinct();

            var institutionData = await GetEducationalInstitutions(regNrs)
                .ListAsync(EServiceMapper.ProjectEducationalInstitution(), cancellationToken: cancellationToken);

            var institutionMapping = institutionData
                .GroupBy(intitution => intitution.Code)
                .Select(group => group.First())
                .ToDictionary(institution => institution.Code, institution => institution);

            var result = data
                .Select(EServiceMapper.ProjectRelatedPerson())
                .ToArray();

            foreach (var student in result)
                foreach (var institution in student.ActiveEducationData)
                    if (institutionMapping.TryGetValue(institution.EducationInstitutionCode, out EServicesEducationalInstitutionDto inst))
                        EServiceMapper.Map(inst, institution);

            await gdprAuditService.TraceRangeAsync(result.Select(GdprAuditHelper.ProjectTraces(type)), cancellationToken);

            return result;
        }

        public async Task<IEnumerable<EServiceEmployeeResponseDto>> GetEmployeesAsync(CancellationToken cancellationToken = default)
        {
            var data = await viisService.GetEmployeesAsync(
                   privatePersonalIdentifier: vraaUser.PrivatePersonalIdentifier,
                   cancellationToken: cancellationToken);

            var regNrs = data.SelectMany(employee => employee.Institution.Select(institution => institution.RegNr))
                .Distinct();

            var institutionData = await GetEducationalInstitutions(regNrs)
                .ListAsync(EServiceMapper.ProjectEducationalInstitution(), cancellationToken: cancellationToken);

            var institutionMapping = institutionData
                .GroupBy(intitution => intitution.Code)
                .Select(group => group.First())
                .ToDictionary(institution => institution.Code, institution => institution);

            var result = data
                .Select(EServiceMapper.ProjectEmployee())
                .ToArray();

            foreach (var employee in result)
                foreach (var institution in employee.ActiveWorkData)
                    if (institutionMapping.TryGetValue(institution.EducationInstitutionCode, out EServicesEducationalInstitutionDto inst))
                        EServiceMapper.Map(inst, institution);

            await gdprAuditService.TraceRangeAsync(result.Select(GdprAuditHelper.ProjectTraces()).ToArray(), cancellationToken);

            return result;
        }


        public static class GdprAuditHelper
        {
            public static Func<EServicesRelatedPersonResponseDto, GdprAuditTraceDto> ProjectTraces(RequestParamType type)
            {
                return dto => new GdprAuditTraceDto
                {
                    Action = "eservices.getRelatedPersonData",
                    ActionData = JsonSerializer.Serialize(new { Type = type }),
                    DataOwnerPrivatePersonalIdentifier = dto.PrivatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.BirthDate, Value = dto.BirthDate },
                        new PersonDataProperty { Type = PersonDataType.FirstName, Value = dto.FirstName },
                        new PersonDataProperty { Type = PersonDataType.LastName, Value = dto.LastName },
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = dto.PrivatePersonalIdentifier },
                    }.Where(t => t.Value != null).ToArray()
                };
            }

            public static Func<EServiceEmployeeResponseDto, GdprAuditTraceDto> ProjectTraces()
            {
                return dto => new GdprAuditTraceDto
                {
                    Action = "eservices.getEmployees",
                    ActionData = null,
                    DataOwnerPrivatePersonalIdentifier = dto.PrivatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.FirstName, Value = dto.FirstName },
                        new PersonDataProperty { Type = PersonDataType.LastName, Value = dto.LastName },
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = dto.PrivatePersonalIdentifier },
                    }.Where(t => t.Value != null).ToArray()
                };
            }
        }
    }
}

