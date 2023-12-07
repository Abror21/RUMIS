using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
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
    [PermissionAuthorize(Permission.ApplicationResourceView)]
    public class ApplicationResourcesController : ApiController
    {
        private readonly IApplicationResourceService service;
        private readonly IClassifierService classifierService;
        private readonly IGdprAuditService gdprAuditService;

        public ApplicationResourcesController(
            IApplicationResourceService service,
            IClassifierService classifierService,
            IGdprAuditService gdprAuditService)
        {
            this.service = service;
            this.classifierService = classifierService;
            this.gdprAuditService = gdprAuditService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedListModel<ApplicationResourceListItemResponse>>> Get([FromQuery] PagingRequest paging = null, [FromQuery] ApplicationResourceFilterRequest filter = null, CancellationToken cancellationToken = default)
        {
            var pagingParams = new PagingParams<ApplicationResource>(paging)
                .AddDefaultSorting(t => t.PNANumber)
                .AddSorting("ApplicationNumber", t => t.Application.ApplicationNumber)
                .AddSorting("AssignedResource", t => t.AssignedResource.Manufacturer.Value + " " + t.AssignedResource.ModelName.Value)
                .AddSorting("AssignedResourceReturnDate", t => t.AssignedResourceReturnDate)
                .AddSorting("DocumentDate", t => t.ApplicationResourceAttachmentList.Where(a => a.DocumentType.Code == DocumentType.PNA).OrderBy(a => a.DocumentDate).Select(a => a.DocumentDate).Last())
                .AddSorting("EducationalInstitution", t => t.Application.EducationalInstitution.Name)
                .AddSorting("InventoryNumber", t => t.AssignedResource.InventoryNumber)
                .AddSorting("IssuedDifferent", t => t.Application.ResourceSubTypeId != t.AssignedResource.ResourceSubTypeId)
                .AddSorting("PNANumber", t => t.PNANumber)
                .AddSorting("PNAStatus", t => t.PNAStatus.Value)
                .AddSorting("ResourceNumber", t => t.AssignedResource.ResourceNumber)
                .AddSorting("ResourceSubTypeId", t => t.AssignedResource.ResourceSubTypeId)
                .AddSorting("ResourceTargetPerson", t => t.Application.ResourceTargetPerson.Persons.OrderBy(t => t.Created).Last().FirstName + " " + t.Application.ResourceTargetPerson.Persons.OrderBy(t => t.Created).Last().LastName)
                .AddSorting("ResourceTargetPersonClass", t => t.Application.ResourceTargetPersonClassGrade.HasValue ?
                                                                t.Application.ResourceTargetPersonClassGrade + " " + t.Application.ResourceTargetPersonClassParallel : t.Application.ResourceTargetPersonGroup)
                .AddSorting("ResourceTargetPersonPrivatePersonalIdentifier", t => t.Application.ResourceTargetPerson.Persons.OrderBy(t => t.Created).Last().PrivatePersonalIdentifier)
                .AddSorting("ResourceTargetPersonStatus", t => t.Application.ResourceTargetPersonEducationalStatusId.HasValue ?
                                                                            t.Application.ResourceTargetPersonEducationalStatusId.Value : t.Application.ResourceTargetPersonWorkStatusId.Value)
                .AddSorting("ResourceTargetPersonType", t => t.Application.ResourceTargetPersonType.Value)
                .AddSorting("SerialNumber", t => t.AssignedResource.SerialNumber)
                .AddSorting("Supervisor", t => t.Application.EducationalInstitution.Supervisor.Name);

            var query = service.Get()
                .Filter(filter);

            var total = await query.CountAsync(cancellationToken);

            var intermediateItems = await query
                .Paging(pagingParams)
                .ListAsync(map: ApplicationResourceMapper.ProjectIntermediateListItem(), cancellationToken: cancellationToken);

            var resourceTypes = await classifierService.Get()
                .Where(t => t.Type == ClassifierTypes.ResourceType)
                .ListAsync(map: ApplicationResourceListItem.ClassifierData.Project(), cancellationToken: cancellationToken);

            var items = intermediateItems.Select(t =>
                {
                    var applicationResourceSubtypePayload = JsonSerializer.Deserialize<ResourceSubTypePayload>(t.Application.ResourceSubType.Payload);
                    var resourceSubtypePayload = t.Resource == null ? null :
                        JsonSerializer.Deserialize<ResourceSubTypePayload>(t.Resource.ResourceSubType.Payload);

                    var response = ApplicationResourceMapper.Map(t, new ApplicationResourceListItemResponse());

                    response.Application.ResourceType = resourceTypes.First(t => t.Code == applicationResourceSubtypePayload.ResourceType);

                    if (resourceSubtypePayload != null)
                        response.Resource.ResourceType = resourceTypes.First(t => t.Code == resourceSubtypePayload.ResourceType);

                    return response;
                })
                .ToList();

            await gdprAuditService.TraceRangeAsync(items.Select(GdprAuditHelper.ProjectTraces()), cancellationToken);

            var result = new PagedListModel<ApplicationResourceListItemResponse>()
            {
                Items = items,
                Page = pagingParams.Page,
                PageSize = pagingParams.PageSize,
                Total = total
            };

            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationResourceResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await service.Get()
                .Where(t => t.Id == id)
                .FirstAsync(ApplicationResourceMapper.Project(), cancellationToken);

            var resourceSubtypePayload = result.AssignedResource == null ? null : JsonSerializer.Deserialize<ResourceSubTypePayload>(result.AssignedResource.ResourceSubType.Payload);

            var resourceType = await classifierService.Get()
                .Where(t => t.Type == ClassifierTypes.ResourceType && t.Code == resourceSubtypePayload.ResourceType)
                .FirstAsync();

            result.AssignedResource.ResourceType = resourceType == null ? null :
                new ApplicationResourceResponse.ClassifierData
                {
                    Id = resourceType.Id,
                    Code = resourceType.Code,
                    Value = resourceType.Value
                };

            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForGetByIdOperation(id, result), cancellationToken);

            return result;
        }

        [HttpGet("{id}/pna")]
        public async Task<ActionResult<string>> GetPna(Guid id, CancellationToken cancellationToken = default)
        {
            return await service.GetPnaAsync(id, cancellationToken);
        }

        [HttpGet("{id}/pna/download")]
        public async Task<IActionResult> DownloadPna(Guid id, CancellationToken cancellationToken = default)
        {
            var file = await service.GetPnaPdfAsync(id, cancellationToken);

            return new FileContentResult(file.Content, file.ContentType)
            {
                FileDownloadName = file.FileName
            };
        }

        [HttpGet("{id}/exploitationRules")]
        public async Task<ActionResult<string>> GetExploitationRules(Guid id, CancellationToken cancellationToken = default)
        {
            return await service.GetExploitationRulesAsync(id, cancellationToken);
        }

        [HttpGet("{id}/exploitationRules/download")]
        public async Task<IActionResult> DownloadExploitationRules(Guid id, CancellationToken cancellationToken = default)
        {
            var file = await service.GetExploitationRulesPdfAsync(id, cancellationToken);

            return new FileContentResult(file.Content, file.ContentType)
            {
                FileDownloadName = file.FileName
            };
        }

        [HttpPut("{id}/lost")]
        [PermissionAuthorize(Permission.ApplicationResourceEdit)]
        public async Task<ActionResult> ChangeStatusToLost(Guid id, [FromForm] ApplicationResourceChangeStatusRequest model, CancellationToken cancellationToken = default)
        {
            await service.ChangeStatusToLostAsync(id, ApplicationResourceMapper.Map(model, new ApplicationResourceChangeStatusDto()), cancellationToken);

            return NoContent();
        }

        [HttpPut("{id}/prepared")]
        [PermissionAuthorize(Permission.ApplicationResourceEdit)]
        public async Task<ActionResult> ChangeStatusToPrepared(Guid id, CancellationToken cancellationToken = default)
        {
            await service.ChangeStatusToPreparedAsync(id, cancellationToken);

            return NoContent();
        }

        [HttpPut("{id}/stolen")]
        [PermissionAuthorize(Permission.ApplicationResourceEdit)]
        public async Task<ActionResult> ChangeStatusToStolen(Guid id, [FromForm] ApplicationResourceChangeStatusRequest model, CancellationToken cancellationToken = default)
        {
            await service.ChangeStatusToStolenAsync(id, ApplicationResourceMapper.Map(model, new ApplicationResourceChangeStatusDto()), cancellationToken);

            return NoContent();
        }

        [HttpPost]
        [PermissionAuthorize(Permission.ApplicationResourceEdit)]
        public async Task<ActionResult<Guid>> CreateWithDraftStatus(ApplicationResourceCreateRequest model, CancellationToken cancellationToken = default)
        {
            return await service.CreateWithDraftStatusAsync(ApplicationResourceMapper.Map(model, new ApplicationResourceCreateDto()), cancellationToken);
        }

        [HttpPost("prepared")]
        [PermissionAuthorize(Permission.ApplicationResourceEdit)]
        public async Task<ActionResult<Guid>> CreateWithPreparedStatus(ApplicationResourceCreateRequest model, CancellationToken cancellationToken = default)
        {
            return await service.CreateWithPreparedStatusAsync(ApplicationResourceMapper.Map(model, new ApplicationResourceCreateDto()), cancellationToken);
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.ApplicationResourceEdit)]
        public async Task<ActionResult> Update(Guid id, ApplicationResourceUpdateRequest model, CancellationToken cancellationToken = default)
        {
            await service.UpdateAsync(id, (ApplicationResourceUpdateDto)ApplicationResourceMapper.Map(model, new ApplicationResourceUpdateDto()), cancellationToken);

            return NoContent();
        }

        [HttpPut("{id}/return")]
        [PermissionAuthorize(Permission.ApplicationResourceEdit)]
        public async Task<ActionResult> Return(Guid id, ApplicationResourceReturnEditRequest model, CancellationToken cancellationToken = default)
        {
            await service.ReturnAsync(id, ApplicationResourceMapper.Map(model, new ApplicationResourceReturnEditDto()), cancellationToken);

            return NoContent();
        }

        [HttpPut("setReturnDeadline")]
        [PermissionAuthorize(Permission.ApplicationResourceEdit)]
        public async Task<ActionResult> SetReturnDeadline(ApplicationResourceReturnDeadlineRequest model, [FromServices] NotificationOptions options, [FromQuery] bool notifyContactPersons = true, CancellationToken cancellationToken = default)
        {
            options.Ignore = !notifyContactPersons;

            await service.SetReturnDeadlineAsync(ApplicationResourceMapper.Map(model, new ApplicationResourceReturnDeadlineDto()), cancellationToken);

            return NoContent();
        }

        [HttpPut("{id}/sign")]
        [PermissionAuthorize(Permission.ApplicationResourceEdit)]
        public async Task<ActionResult> Sign(Guid id, CancellationToken cancellationToken = default)
        {
            await service.SignAsync(id, cancellationToken);

            return NoContent();
        }

        [HttpPut("{id}/cancel")]
        [PermissionAuthorize(Permission.ApplicationResourceEdit)]
        public async Task<ActionResult> Cancel(Guid id, ApplicationResourceCancelRequest model, CancellationToken cancellationToken = default)
        {
            await service.CancelAsync(id, ApplicationResourceMapper.Map(model, new ApplicationResourceCancelDto()), cancellationToken);

            return NoContent();
        }


        public static class GdprAuditHelper
        {
            public static GdprAuditTraceDto GenerateTraceForGetByIdOperation(Guid applicationResourceId, ApplicationResourceResponse model)
            {
                var result = new GdprAuditTraceDto
                {
                    Action = "applicationResource.getById",
                    ActionData = JsonSerializer.Serialize(new { ApplicationResourceId = applicationResourceId }),
                    DataOwnerId = model.Application.ResourceTargetPerson.Id,
                    EducationalInstitutionId = model.Application.EducationalInstitution.Id
                };

                var data = new List<PersonDataProperty>();

                foreach (var person in model.Application.ResourceTargetPerson.Persons)
                {
                    data.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                    data.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                    data.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                }

                result.Data = data.Where(t => t.Value != null)
                    .ToArray();

                return result;
            }

            public static Func<ApplicationResourceListItemResponse, GdprAuditTraceDto> ProjectTraces()
            {
                return response =>
                {
                    var result = new GdprAuditTraceDto
                    {
                        Action = "applicationResource.getList",
                        ActionData = null,
                        DataOwnerId = response.Application.ResourceTargetPerson.Id,
                        EducationalInstitutionId = response.Application.EducationalInstitution.Id
                    };

                    var data = new List<PersonDataProperty>();

                    foreach (var person in response.Application.ResourceTargetPerson.Persons)
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
