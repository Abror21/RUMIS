using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using Izm.Rumis.Infrastructure.Notifications;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    [PermissionAuthorize(Permission.ApplicationView)]
    public class ApplicationsController : ApiController
    {
        private readonly IApplicationService applicationService;
        private readonly IClassifierService classifierService;
        private readonly IGdprAuditService gdprAuditService;

        public ApplicationsController(
            IApplicationService applicationService,
            IClassifierService classifierService,
            IGdprAuditService gdprAuditService)
        {
            this.applicationService = applicationService;
            this.classifierService = classifierService;
            this.gdprAuditService = gdprAuditService;
        }

        [HttpGet("{id}/applicationSocialStatus")]
        public async Task<ActionResult<ApplicationSocialStatusResponse>> CheckApplicationSocialStatus(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await applicationService.CheckApplicationSocialStatusAsync(id, cancellationToken);

            return ApplicationMapper.Map(entity, new ApplicationSocialStatusResponse());
        }

        [HttpGet("checkDuplicate")]
        public async Task<ActionResult<ApplicationCheckDuplicateResponse>> CheckApplicationDuplicate([FromQuery] ApplicationCheckDuplicateRequest model, CancellationToken cancellationToken = default)
        {
            return await applicationService.GetApplicationDuplicates(ApplicationMapper.Map(model, new ApplicationCheckDuplicateDto()))
                        .FirstAsync(ApplicationMapper.ProjectDuplicate(), cancellationToken);
        }

        [HttpPost]
        [PermissionAuthorize(Permission.ApplicationEdit)]
        public async Task<ActionResult<ApplicationCreateResponse>> Create(ApplicationCreateRequest model, CancellationToken cancellationToken = default)
        {
            var result = await applicationService.CreateAsync(ApplicationMapper.Map(model, new ApplicationCreateDto()), cancellationToken);

            return ApplicationMapper.Map(result, new ApplicationCreateResponse());
        }

        [HttpGet]
        public async Task<ActionResult<PagedListModel<ApplicationListItemResponse>>> Get([FromQuery] PagingRequest paging = null, [FromQuery] ApplicationFilterRequest filter = null, CancellationToken cancellationToken = default)
        {
            var pagingParams = new PagingParams<Domain.Entities.Application>(paging)
                .SetMaxPageSize(100)
                .AddDefaultSorting(t => t.ApplicationNumber)
                .AddSorting("ApplicationDate", t => t.ApplicationDate)
                .AddSorting("ApplicationNumber", t => t.ApplicationNumber)
                .AddSorting("ApplicationSocialStatus", t => t.ApplicationSocialStatuses.OrderBy(t => t.Created).Last().SocialStatus.Value)
                .AddSorting("ApplicationStatus", t => t.ApplicationStatus.Value)
                .AddSorting("ApplicationStatusHistory", t => t.ApplicationStatusHistory)
                .AddSorting("ContactPerson", t => t.ContactPerson.Persons.OrderBy(t => t.Created).Last().FirstName + " " + t.ContactPerson.Persons.OrderBy(t => t.Created).Last().LastName)
                .AddSorting("Created", t => t.Created)
                .AddSorting("CreatedById", t => t.CreatedById)
                .AddSorting("EducationalInstitution", t => t.EducationalInstitution.Name)
                .AddSorting("Supervisor", t => t.EducationalInstitution.Supervisor.Name)
                .AddSorting("Id", t => t.Id)
                .AddSorting("Modified", t => t.Modified)
                .AddSorting("ModifiedById", t => t.ModifiedById)
                .AddSorting("Notes", t => t.Notes)
                .AddSorting("ResourceSubType", t => t.ResourceSubType.Value)
                .AddSorting("ResourceTargetPerson", t => t.ResourceTargetPerson.Persons.OrderBy(t => t.Created).Last().FirstName + " " + t.ResourceTargetPerson.Persons.OrderBy(t => t.Created).Last().LastName)
                .AddSorting("ResourceTargetPersonClassGrade", t => t.ResourceTargetPersonClassGrade)
                .AddSorting("ResourceTargetPersonClassParallel", t => t.ResourceTargetPersonClassParallel)
                .AddSorting("ResourceTargetPersonEducationalProgram", t => t.ResourceTargetPersonEducationalProgram)
                .AddSorting("ResourceTargetPersonEducationalStatus", t => t.ResourceTargetPersonEducationalStatus.Value)
                .AddSorting("ResourceTargetPersonEducationalSubStatus", t => t.ResourceTargetPersonEducationalSubStatus.Value)
                .AddSorting("ResourceTargetPersonGroup", t => t.ResourceTargetPersonGroup)
                .AddSorting("ResourceTargetPersonType", t => t.ResourceTargetPersonType.Value)
                .AddSorting("ResourceTargetPersonWorkStatus", t => t.ResourceTargetPersonWorkStatus.Value)
                .AddSorting("SocialStatus", t => t.SocialStatus)
                .AddSorting("SubmitterPerson", t => t.SubmitterPerson.Persons.OrderBy(t => t.Created).Last().FirstName + " " + t.SubmitterPerson.Persons.OrderBy(t => t.Created).Last().LastName)
                .AddSorting("SubmitterType", t => t.SubmitterType.Value);

            var query = applicationService.Get()
                .Filter(filter);

            var total = await query.CountAsync(cancellationToken);

            var intermediateItems = await query
                .Paging(pagingParams)
                .ListAsync(map: ApplicationMapper.ProjectIntermediateListItem(), cancellationToken: cancellationToken);

            var resourceTypes = await classifierService.Get()
                .Where(t => t.Type == ClassifierTypes.ResourceType)
                .ListAsync(map: ApplicationMapper.ProjectApplicationListItemClassifier(), cancellationToken: cancellationToken);

            var items = intermediateItems.Select(t =>
                {
                    var resourceSubtypePayload = JsonSerializer.Deserialize<ResourceSubTypePayload>(t.ResourceSubType.Payload);

                    var response = new ApplicationListItemResponse();

                    response.ResourceType = resourceTypes.First(t => t.Code == resourceSubtypePayload.ResourceType);

                    return ApplicationMapper.Map(t, response);
                })
                .ToList();

            await gdprAuditService.TraceRangeAsync(items.Select(GdprAuditHelper.ProjectListItemTraces()).SelectMany(t => t));

            var result = new PagedListModel<ApplicationListItemResponse>()
            {
                Items = items,
                Page = pagingParams.Page,
                PageSize = pagingParams.PageSize,
                Total = total
            };

            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await applicationService.Get()
                .Where(t => t.Id == id)
                .FirstAsync(ApplicationMapper.Project(), cancellationToken);

            await gdprAuditService.TraceRangeAsync(GdprAuditHelper.GenerateTracesForGetByIdOperation(id, result));

            return result;
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.ApplicationEdit)]
        public async Task<ActionResult> Update(Guid id, ApplicationUpdateRequest model, CancellationToken cancellationToken = default)
        {
            await applicationService.UpdateAsync(id, ApplicationMapper.Map(model, new ApplicationUpdateDto()), cancellationToken);

            return NoContent();
        }

        [HttpPut("decline/{notifyContactPersons}")]
        [PermissionAuthorize(Permission.ApplicationEdit)]
        public async Task<ActionResult> Decline(ApplicationDeclineRequest model, bool notifyContactPersons, [FromServices] NotificationOptions options, CancellationToken cancellationToken = default)
        {
            options.Ignore = !notifyContactPersons;

            await applicationService.DeclineAsync(ApplicationMapper.Map(model, new ApplicationDeclineDto()), cancellationToken);

            return NoContent();
        }

        [HttpPut("postpone/{id}")]
        [PermissionAuthorize(Permission.ApplicationEdit)]
        public async Task<ActionResult> Postpone(Guid id, CancellationToken cancellationToken = default)
        {
            await applicationService.PostponeAsync(id, cancellationToken);

            return NoContent();
        }

        [HttpDelete("{notifyContactPersons}")]
        [PermissionAuthorize(Permission.ApplicationEdit)]
        public async Task<ActionResult> Delete(IEnumerable<Guid> applicationIds, bool notifyContactPersons, [FromServices] NotificationOptions options, CancellationToken cancellationToken = default)
        {
            options.Ignore = !notifyContactPersons;

            await applicationService.DeleteAsync(applicationIds, cancellationToken);

            return NoContent();
        }

        [HttpPut("{id}/becomeContact")]
        [PermissionAuthorize(Permission.ApplicationEdit)]
        public async Task<ActionResult> UpdateApplicationContactPersonInformation(Guid id, ApplicationContactPersonUpdateRequest model, CancellationToken cancellationToken = default)
        {
            await applicationService.ChangeSubmitterContactAsync(
                id,
                item: ApplicationMapper.Map(model, new ApplicationContactInformationUpdateDto()),
                cancellationToken: cancellationToken
                );

            return NoContent();
        }

        [HttpPut("becomeContact")]
        [PermissionAuthorize(Permission.ApplicationEdit)]
        public async Task<ActionResult> UpdateApplicationsContactPersonInformation(ApplicationsContactPersonUpdateRequest model, CancellationToken cancellationToken = default)
        {
            var applicationIds = await applicationService.Get()
                .Where(t => t.ResourceTargetPersonId == model.ResourceTargetPersonId)
                .ListAsync(map: t => t.Id, cancellationToken: cancellationToken);

            await applicationService.ChangeSubmittersContactAsync(
                ids: applicationIds,
                item: ApplicationMapper.Map(model, new ApplicationsContactInformationUpdateDto()),
                cancellationToken: cancellationToken
                );

            return NoContent();
        }

        public static class GdprAuditHelper
        {
            public static IEnumerable<GdprAuditTraceDto> GenerateTracesForGetByIdOperation(Guid applicationId, ApplicationResponse model)
            {
                var result = new List<GdprAuditTraceDto>();

                var contactPersonTrace = new GdprAuditTraceDto
                {
                    Action = "application.getById",
                    ActionData = JsonSerializer.Serialize(new { ApplicationId = applicationId }),
                    DataOwnerId = model.ContactPerson.Id,
                    EducationalInstitutionId = model.EducationalInstitution.Id
                };

                var contactPersonData = new List<PersonDataProperty>();

                foreach (var person in model.ContactPerson.Person)
                {
                    contactPersonData.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                    contactPersonData.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                    contactPersonData.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                }

                foreach (var contact in model.ContactPerson.ContactData)
                    contactPersonData.Add(new PersonDataProperty { Type = PersonDataType.Contact, Value = contact.Value });

                contactPersonData = contactPersonData.Where(t => t.Value != null)
                    .ToList();

                result.Add(contactPersonTrace);

                if (model.ResourceTargetPerson.Id != model.ContactPerson.Id)
                {
                    var resourceTargetPersonTrace = new GdprAuditTraceDto
                    {
                        Action = "application.getList",
                        ActionData = null,
                        DataOwnerId = model.ResourceTargetPerson.Id,
                        EducationalInstitutionId = model.EducationalInstitution.Id
                    };

                    var resourceTargetPersonData = new List<PersonDataProperty>();

                    foreach (var person in model.ResourceTargetPerson.Person)
                    {
                        resourceTargetPersonData.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                        resourceTargetPersonData.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                        resourceTargetPersonData.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                    }

                    resourceTargetPersonData = resourceTargetPersonData.Where(t => t.Value != null)
                        .ToList();

                    result.Add(resourceTargetPersonTrace);
                }

                if (model.SubmitterPerson.Id != model.ContactPerson.Id && model.SubmitterPerson.Id != model.ResourceTargetPerson.Id)
                {
                    var submitterPersonTrace = new GdprAuditTraceDto
                    {
                        Action = "application.getList",
                        ActionData = null,
                        DataOwnerId = model.SubmitterPerson.Id,
                        EducationalInstitutionId = model.EducationalInstitution.Id
                    };

                    var submitterPersonData = new List<PersonDataProperty>();

                    foreach (var person in model.SubmitterPerson.Person)
                    {
                        submitterPersonData.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                        submitterPersonData.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                        submitterPersonData.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                    }

                    submitterPersonData = submitterPersonData.Where(t => t.Value != null)
                        .ToList();

                    result.Add(submitterPersonTrace);
                }

                return result;
            }

            public static Func<ApplicationListItemResponse, IEnumerable<GdprAuditTraceDto>> ProjectListItemTraces()
            {
                return response =>
                {
                    var result = new List<GdprAuditTraceDto>();

                    var contactPersonTrace = new GdprAuditTraceDto
                    {
                        Action = "application.getList",
                        ActionData = null,
                        DataOwnerId = response.ContactPerson.Id,
                        EducationalInstitutionId = response.EducationalInstitution.Id
                    };

                    var contactPersonData = new List<PersonDataProperty>();

                    foreach (var person in response.ContactPerson.Person)
                    {
                        contactPersonData.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                        contactPersonData.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                        contactPersonData.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                    }

                    foreach (var contact in response.ContactPerson.Contacts)
                        contactPersonData.Add(new PersonDataProperty { Type = PersonDataType.Contact, Value = contact.ContactValue });

                    contactPersonData = contactPersonData.Where(t => t.Value != null)
                        .ToList();

                    result.Add(contactPersonTrace);

                    if (response.ResourceTargetPerson.Id != response.ContactPerson.Id)
                    {
                        var resourceTargetPersonTrace = new GdprAuditTraceDto
                        {
                            Action = "application.getList",
                            ActionData = null,
                            DataOwnerId = response.ResourceTargetPerson.Id,
                            EducationalInstitutionId = response.EducationalInstitution.Id
                        };

                        var resourceTargetPersonData = new List<PersonDataProperty>();

                        foreach (var person in response.ResourceTargetPerson.Person)
                        {
                            resourceTargetPersonData.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                            resourceTargetPersonData.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                            resourceTargetPersonData.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                        }

                        resourceTargetPersonData = resourceTargetPersonData.Where(t => t.Value != null)
                            .ToList();

                        result.Add(resourceTargetPersonTrace);
                    }

                    if (response.SubmitterPerson.Id != response.ContactPerson.Id && response.SubmitterPerson.Id != response.ResourceTargetPerson.Id)
                    {
                        var submitterPersonTrace = new GdprAuditTraceDto
                        {
                            Action = "application.getList",
                            ActionData = null,
                            DataOwnerId = response.SubmitterPerson.Id,
                            EducationalInstitutionId = response.EducationalInstitution.Id
                        };

                        var submitterPersonData = new List<PersonDataProperty>();

                        foreach (var person in response.SubmitterPerson.Person)
                        {
                            submitterPersonData.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                            submitterPersonData.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                            submitterPersonData.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                        }

                        submitterPersonData = submitterPersonData.Where(t => t.Value != null)
                            .ToList();

                        result.Add(submitterPersonTrace);
                    }

                    return result;
                };
            }
        }
    }
}
