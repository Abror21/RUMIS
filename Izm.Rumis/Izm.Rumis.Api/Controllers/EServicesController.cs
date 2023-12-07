using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using Izm.Rumis.Infrastructure.Enums;
using Izm.Rumis.Infrastructure.EServices.Dtos;
using Izm.Rumis.Infrastructure.Services;
using Izm.Rumis.Infrastructure.Viis;
using Izm.Rumis.Infrastructure.Vraa;
#if !DEBUG
using Microsoft.AspNetCore.Authorization;
#endif
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
#if !DEBUG
    [Authorize(AuthenticationSchemes = VraaDefaults.AuthenticationScheme)]
#endif
    public sealed class EServicesController : ApiController
    {
        private readonly IVraaUser vraaUser;
        private readonly IViisService viisService;
        private readonly IEServicesService eServicesService;
        private readonly IGdprAuditService gdprAuditService;
        private readonly IClassifierService classifierService;

        public EServicesController(
            IViisService viisService,
            IEServicesService eServicesService,
            IVraaUser vraaUser,
            IClassifierService classifierService,
            IGdprAuditService gdprAuditService)
        {
            this.viisService = viisService;
            this.eServicesService = eServicesService;
            this.vraaUser = vraaUser;
            this.classifierService = classifierService;
            this.gdprAuditService = gdprAuditService;
        }

        [HttpGet("asParentOrGuardian")]
        public async Task<ActionResult<IEnumerable<EServiceRelatedPersonResponse>>> GetAsParentOrGuardian(CancellationToken cancellationToken = default)
        {
            var data = await eServicesService.GetAsParentOrGuardian(cancellationToken);

            return data.Select(EServiceMapper.ProjectRelatedPerson())
                .ToArray();
        }

        [HttpGet("asEducatee")]
        public async Task<ActionResult<IEnumerable<EServiceRelatedPersonResponse>>> GetAsEducatee(CancellationToken cancellationToken = default)
        {
            var data = await eServicesService.GetAsEducatee(cancellationToken);

            return data.Select(EServiceMapper.ProjectRelatedPerson())
                .ToArray();
        }

        [HttpGet("asEmployee")]
        public async Task<ActionResult<IEnumerable<EServiceEmployeeResponse>>> GetAsEmployee(CancellationToken cancellationToken = default)
        {
            var data = await eServicesService.GetEmployeesAsync(cancellationToken);

            return data.Select(EServiceMapper.ProjectEmployee())
                .ToArray();
        }

        [HttpGet("applications")]
        public async Task<ActionResult<IEnumerable<EServiceApplicationResponseData>>> GetApplications(CancellationToken cancellationToken = default)
        {
            var data = await viisService.GetStudentsAsync(RequestParamType.Parent, vraaUser.PrivatePersonalIdentifier, cancellationToken);

            var privatePersonalIdentifiers = data.Select(x => x.PersonCode).ToList();

            privatePersonalIdentifiers.Add(vraaUser.PrivatePersonalIdentifier);

            var applications = await eServicesService.GetApplications(privatePersonalIdentifiers)
                .ListAsync(map: EServiceMapper.ProjectApplicationListItem(), cancellationToken: cancellationToken);

            var result = applications.Select(EServiceApplicationResponseData.Project())
                .OrderByDescending(t => t.ApplicationDate)
                .ToArray();

            await gdprAuditService.TraceRangeAsync(result.Select(GdprAuditHelper.ProjectTraces()), cancellationToken);

            return result;
        }

        [HttpGet("applications/{id}")]
        public async Task<ActionResult<EServiceApplicationResponse>> GetApplicationById(Guid id, CancellationToken cancellationToken = default)
        {
            var data = await viisService.GetStudentsAsync(RequestParamType.Parent, vraaUser.PrivatePersonalIdentifier, cancellationToken);

            var privatePersonalIdentifiers = data.Select(x => x.PersonCode).ToList();

            privatePersonalIdentifiers.Add(vraaUser.PrivatePersonalIdentifier);

            var intermediateApplication = await eServicesService.GetApplications(privatePersonalIdentifiers)
                .Where(t => t.Id == id)
                .FirstAsync(map: EServiceMapper.ProjectIntermediateApplication(), cancellationToken: cancellationToken);

            if (intermediateApplication == null)
                return NotFound();

            var result = EServiceMapper.Map(intermediateApplication, new EServiceApplicationResponse());

            var contactPerson = intermediateApplication.ContactPerson.Persons.OrderBy(t => t.ActiveFrom).Last();

            if (vraaUser.PrivatePersonalIdentifier == contactPerson.PrivatePersonalIdentifier)
                result = EServiceMapper.MapContactPerson(intermediateApplication, result);

            var applicationResourceSubtypePayload = JsonSerializer.Deserialize<ResourceSubTypePayload>(intermediateApplication.ResourceSubType.Payload);

            var applicationResourceTypeData = await classifierService.Get()
                .Where(t => t.Type == ClassifierTypes.ResourceType && t.Code == applicationResourceSubtypePayload.ResourceType)
                .FirstAsync(map: EServiceMapper.ProjectApplicationResponseClassifier(), cancellationToken: cancellationToken);

            result.ResourceType = applicationResourceTypeData;

            var applicationResource = intermediateApplication.ApplicationResources.Any(n => PnaStatus.ActiveStatuses.Contains(n.PNAStatus.Code))
                ? intermediateApplication.ApplicationResources
                    .Where(n => PnaStatus.ActiveStatuses.Contains(n.PNAStatus.Code))
                    .FirstOrDefault()
                : intermediateApplication.ApplicationResources
                    .Where(n => PnaStatus.NonActiveStatuses.Contains(n.PNAStatus.Code))
                    .OrderBy(n => n.Created)
                    .LastOrDefault();

            if (applicationResource != null && applicationResource.Resource.ResourceSubType != null)
            {
                var resourceSubtypePayload = JsonSerializer.Deserialize<ResourceSubTypePayload>(applicationResource.Resource.ResourceSubType.Payload);

                var resourceTypeData = await classifierService.Get()
                    .Where(t => t.Type == ClassifierTypes.ResourceType && t.Code == resourceSubtypePayload.ResourceType)
                    .FirstAsync(map: EServiceMapper.ProjectApplicationResponseClassifier(), cancellationToken: cancellationToken);

                var responseApplicationResource = EServiceMapper.MapApplicationResource(applicationResource, new EServiceApplicationResponse.ApplicationResourceDataExpanded());

                responseApplicationResource.Resource.ResourceType = resourceTypeData;

                result.ApplicationResources = new[] { responseApplicationResource };
            }

            await gdprAuditService.TraceRangeAsync(GdprAuditHelper.GenerateTracesForGetApplicationByIdOperation(id, result), cancellationToken);

            return result;
        }

        [HttpPost("applications/checkDuplicate")]
        public async Task<ActionResult<EServiceApplicationCheckDuplicateResponse>> CheckApplicationDuplicate(EServiceApplicationCheckDuplicateRequest model, CancellationToken cancellationToken = default)
        {
            var query = await eServicesService
                .GetApplicationDuplicatesAsync(EServiceMapper.Map(model, new EServiceApplicationCheckDuplicateDto()), cancellationToken: cancellationToken);

            return await query
                .FirstAsync(EServiceMapper.ProjectApplicationDuplicate(), cancellationToken: cancellationToken);
        }

        [HttpPost("application")]
        public async Task<ActionResult<EServiceApplicationCreateResponse>> CreateApplication(EServiceApplicationCreateRequest model, CancellationToken cancellationToken = default)
        {
            var result = await eServicesService.CreateApplicationAsync(
                item: EServiceMapper.Map(model, new EServiceApplicationCreateDto()),
                cancellationToken: cancellationToken
                );

            return EServiceMapper.Map(result, new EServiceApplicationCreateResponse());
        }

        [HttpPut("applications/{id}/withdraw")]
        public async Task<ActionResult> WithdrawApplication(Guid id, CancellationToken cancellationToken = default)
        {
            await eServicesService.WithdrawApplicationAsync(id, cancellationToken);

            return NoContent();
        }

        [HttpPut("applications/{id}/becomeContact")]
        public async Task<ActionResult> UpdateApplicationContactPersonInformation(Guid id, EServiceContactPersonUpdateRequest model, CancellationToken cancellationToken = default)
        {
            await eServicesService.ChangeSubmitterContactAsync(
                id,
                item: EServiceMapper.Map(model, new EServiceApplicationChangeContactPersonDto()),
                cancellationToken: cancellationToken
                );

            return NoContent();
        }

        [HttpPut("applications/becomeContact")]
        public async Task<ActionResult> UpdateApplicationsContactPersonInformation(EServiceApplicationsContactPersonUpdateRequest model, CancellationToken cancellationToken = default)
        {
            var data = await viisService.GetStudentsAsync(RequestParamType.Parent, vraaUser.PrivatePersonalIdentifier, cancellationToken);

            var privatePersonalIdentifiers = data.Select(x => x.PersonCode).ToList();

            privatePersonalIdentifiers.Add(vraaUser.PrivatePersonalIdentifier);

            var applicationIds = await eServicesService.GetApplications(privatePersonalIdentifiers)
                .Where(t => t.ResourceTargetPersonId == model.ResourceTargetPersonId)
                .ListAsync(map: t => t.Id, cancellationToken: cancellationToken);

            await eServicesService.ChangeSubmittersContactAsync(
                ids: applicationIds,
                item: EServiceMapper.Map(model, new EServiceApplicationsChangeContactPersonDto()),
                cancellationToken: cancellationToken
                );

            return NoContent();
        }

        [HttpGet("applicationResources/pna/sample")]
        public async Task<ActionResult<string>> GetApplicationResourcePnaSample(int educationalInstitutionId, CancellationToken cancellationToken = default)
        {
            return await eServicesService.GetDocumentSampleAsync(
                educationalInstitutionId,
                DocumentType.PNA,
                cancellationToken);
        }

        [HttpGet("applicationResources/{id}/pna")]
        public async Task<ActionResult<string>> GetApplicationResourcePna(Guid id, CancellationToken cancellationToken = default)
        {
            return await eServicesService.GetApplicationResourcePnaAsync(id, cancellationToken);
        }

        [HttpGet("applicationResources/{id}/pna/download")]
        public async Task<IActionResult> DownloadApplicationResourcePna(Guid id, CancellationToken cancellationToken = default)
        {
            var file = await eServicesService.GetApplicationResourcePnaPdfAsync(id, cancellationToken);

            return new FileContentResult(file.Content, file.ContentType)
            {
                FileDownloadName = file.FileName
            };
        }

        [HttpGet("applicationResources/exploitationRules/sample")]
        public async Task<ActionResult<string>> GetApplicationResourceExploitationRulesSample(int educationalInstitutionId, CancellationToken cancellationToken = default)
        {
            return await eServicesService.GetDocumentSampleAsync(
                educationalInstitutionId,
                DocumentType.ExploitationRules,
                cancellationToken);
        }

        [HttpGet("applicationResources/{id}/exploitationRules")]
        public async Task<ActionResult<string>> GetApplicationResourceExploitationRules(Guid id, CancellationToken cancellationToken = default)
        {
            return await eServicesService.GetApplicationResourceExploitationRulesAsync(id, cancellationToken);
        }

        [HttpGet("applicationResources/{id}/exploitationRules/download")]
        public async Task<IActionResult> DownloadApplicationResourceExploitationRules(Guid id, CancellationToken cancellationToken = default)
        {
            var file = await eServicesService.GetApplicationResourceExploitationRulesPdfAsync(id, cancellationToken);

            return new FileContentResult(file.Content, file.ContentType)
            {
                FileDownloadName = file.FileName
            };
        }

        [HttpPut]
        [Route("applicationResource/{id}/lost")]
        [Route("applicationResources/{id}/lost")]
        public async Task<ActionResult> ChangeStatusToLost(Guid id, EServiceChangeStatusRequest model, CancellationToken cancellationToken = default)
        {
            await eServicesService.ChangeStatusToLostAsync(id, EServiceMapper.Map(model, new EServiceChangeStatusDto()), cancellationToken);

            return NoContent();
        }

        [HttpPut]
        [Route("applicationResource/{id}/stolen")]
        [Route("applicationResources/{id}/stolen")]
        public async Task<ActionResult> ChangeStatusToStolen(Guid id, EServiceChangeStatusRequest model, CancellationToken cancellationToken = default)
        {
            await eServicesService.ChangeStatusToStolenAsync(id, EServiceMapper.Map(model, new EServiceChangeStatusDto()), cancellationToken);

            return NoContent();
        }

        [HttpPut("applicationResources/{id}/sign")]
        public async Task<ActionResult> SignApplicationResource(Guid id, CancellationToken cancellationToken = default)
        {
            await eServicesService.SignApplicationResourceAsync(
                id,
                cancellationToken: cancellationToken
                );

            return NoContent();
        }

        [HttpGet("classifiers/getByType")]
        public async Task<ActionResult<IEnumerable<EServiceClassifierResponse>>> GetByType([FromQuery] IEnumerable<string> types, CancellationToken cancellationToken = default)
        {
            var data = await eServicesService.GetClassifiers(types.Select(t => t.ToLower()))
                .OrderBy(t => t.SortOrder)
                .ListAsync(map: EServiceMapper.ProjectClassifier(), cancellationToken: cancellationToken);

            return data.ToList();
        }

        [HttpPost("documentTemplates")]
        public async Task<ActionResult<IEnumerable<EServiceDocumentTemplateResponse>>> GetDocumentTemplates(EServiceDocumentTemplatesRequest model, CancellationToken cancellationToken = default)
        {
            return await eServicesService.GetDocumentTemplates(EServiceMapper.Map(model, new EServiceDocumentTemplatesDto()))
                .ListAsync(map: EServiceMapper.ProjectDocumentTemplate(), cancellationToken: cancellationToken);
        }

        [HttpGet("documentTemplates/{id}")]
        public async Task<ActionResult<string>> GetDocumentTemplateById(int id, CancellationToken cancellationToken = default)
        {
            return await eServicesService.GetDocumentTemplateAsync(id, cancellationToken);
        }

        [HttpGet("documentTemplates/{id}/download")]
        public async Task<IActionResult> DownloadDocumentTemplateById(int id, CancellationToken cancellationToken = default)
        {
            var file = await eServicesService.GetDocumentTemplatePdfAsync(id, cancellationToken);

            return new FileContentResult(file.Content, file.ContentType)
            {
                FileDownloadName = file.FileName
            };
        }

        public static class GdprAuditHelper
        {
            public static IEnumerable<GdprAuditTraceDto> GenerateTracesForGetApplicationByIdOperation(Guid applicationId, EServiceApplicationResponse response)
            {
                var result = new List<GdprAuditTraceDto>();

                var resourceTargetPersonTrace = new GdprAuditTraceDto
                {
                    Action = "eservices.getApplicationById",
                    ActionData = JsonSerializer.Serialize(new { ApplicationId = applicationId }),
                    DataOwnerId = response.ResourceTargetPerson.Id,
                    EducationalInstitutionId = response.EducationalInstitution.Id
                };

                var resourceTargetData = new List<PersonDataProperty>();

                foreach (var person in response.ResourceTargetPerson.Persons)
                {
                    resourceTargetData.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                    resourceTargetData.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                    resourceTargetData.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                }

                resourceTargetPersonTrace.Data = resourceTargetData.Where(t => t.Value != null)
                    .ToArray();

                result.Add(resourceTargetPersonTrace);

                if (response.ContactPerson != null && response.ResourceTargetPerson.Id != response.ContactPerson.Id)
                {
                    var contactPerson = new GdprAuditTraceDto
                    {
                        Action = "eservices.getApplicationById",
                        ActionData = JsonSerializer.Serialize(new { ApplicationId = applicationId }),
                        DataOwnerId = response.ContactPerson.Id,
                        EducationalInstitutionId = response.EducationalInstitution.Id
                    };

                    var contactPersonData = new List<PersonDataProperty>();

                    foreach (var person in response.ContactPerson.Persons)
                    {
                        contactPersonData.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                        contactPersonData.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                        contactPersonData.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                    }

                    foreach (var contact in response.ContactPerson.ContactInformation)
                        contactPersonData.Add(new PersonDataProperty { Type = PersonDataType.Contact, Value = contact.Value });

                    contactPerson.Data = contactPersonData.Where(t => t.Value != null)
                        .ToArray();

                    result.Add(contactPerson);
                }

                return result;
            }

            public static Func<EServiceApplicationResponseData, GdprAuditTraceDto> ProjectTraces()
            {
                return response =>
                {
                    var result = new GdprAuditTraceDto
                    {
                        Action = "eservices.getApplicationList",
                        ActionData = JsonSerializer.Serialize(new { ApplicationId = response.Id }),
                        DataOwnerId = response.ResourceTargetPerson.Id,
                        EducationalInstitutionId = response.EducationalInstitution.Id
                    };

                    var data = new List<PersonDataProperty>();

                    foreach (var person in response.ResourceTargetPerson.Person)
                    {
                        data.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                        data.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                        data.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                    }

                    result.Data = data.Where(t => t.Value != null)
                        .ToArray();

                    return result;
                };
            }
        }
    }
}
