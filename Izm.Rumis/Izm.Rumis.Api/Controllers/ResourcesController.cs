using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using Izm.Rumis.Infrastructure.ResourceImport;
using Izm.Rumis.Infrastructure.ResourceImport.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    [PermissionAuthorize(Permission.ResourceView)]
    public class ResourcesController : ApiController
    {
        private readonly IResourceService service;
        private readonly IResourceImportService resourceImportService;
        private readonly IClassifierService classifierService;

        public ResourcesController(
            IResourceService service,
            IResourceImportService resourceImportService,
            IClassifierService classifierService
            )
        {
            this.service = service;
            this.resourceImportService = resourceImportService;
            this.classifierService = classifierService;
        }

        [HttpPost]
        [PermissionAuthorize(Permission.ResourceEdit)]
        public async Task<ActionResult<Guid>> Create(ResourceCreateRequest model, CancellationToken cancellationToken = default)
        {
            return await service.CreateAsync(ResourceMapper.Map(model, new ResourceCreateDto()), cancellationToken);
        }


        [HttpPost("importData")]
        [PermissionAuthorize(Permission.ResourceEdit)]
        public async Task<ActionResult<ResourceImportDataResponse>> ImportData([FromForm] ResourceImportDataRequest model, CancellationToken cancellationToken = default)
        {
            var result = await resourceImportService.ImportAsync(ResourceMapper.Map(model, new ResourceImportDataDto()), cancellationToken);

            return ResourceMapper.Map(result, new ResourceImportDataResponse());
        }

        [HttpGet]
        public async Task<ActionResult<PagedListModel<ResourceResponse>>> Get([FromQuery] ResourceFilterRequest filter = null, [FromQuery] PagingRequest paging = null, CancellationToken cancellationToken = default)
        {
            var pagingParams = new PagingParams<Resource>(paging)
                .AddDefaultSorting(t => t.Created, SortDirection.Desc)
                .AddSorting("ResourceNumber", t => t.ResourceNumber)
                .AddSorting("ResourceName", t => t.ResourceName)
                .AddSorting("ModelIdentifier", t => t.ModelIdentifier)
                .AddSorting("AcquisitionsValue", t => t.AcquisitionsValue)
                .AddSorting("ManufactureYear", t => t.ManufactureYear)
                .AddSorting("InventoryNumber", t => t.InventoryNumber)
                .AddSorting("SerialNumber", t => t.SerialNumber)
                .AddSorting("ResourceStatusHistory", t => t.ResourceStatusHistory)
                .AddSorting("Notes", t => t.Notes)
                .AddSorting("EducationalInstitution", t => t.EducationalInstitution.Name)
                .AddSorting("ResourceSubType", t => t.ResourceSubType.Value)
                .AddSorting("ResourceGroup", t => t.ResourceGroup.Value)
                .AddSorting("ResourceStatus", t => t.ResourceStatus.Value)  
                .AddSorting("ResourceLocation", t => t.ResourceLocation.Value)
                .AddSorting("TargetGroup", t => t.TargetGroup.Value)
                .AddSorting("UsagePurposeType", t => t.UsagePurposeType.Value)
                .AddSorting("AcquisitionType", t => t.AcquisitionType.Value)
                .AddSorting("Manufacturer", t => t.Manufacturer.Value)
                .AddSorting("ModelName", t => t.ModelName.Value)
                .AddSorting("Supervisor", t => t.EducationalInstitution.Supervisor.Name)
                .AddSorting("SocialSupportResource", t => t.SocialSupportResource);

            var query = service.Get().Filter(filter);

            var total = await query.CountAsync(cancellationToken);

            var intermediateItems = await query
                .Paging(pagingParams)
                .ListAsync(map: ResourceMapper.IntermediateProject(), cancellationToken: cancellationToken);

            var resourceTypes = await classifierService.Get()
                .Where(t => t.Type == ClassifierTypes.ResourceType)
                .ListAsync(map: ResourceResponse.ClassifierData.Project(), cancellationToken: cancellationToken);

            var items = intermediateItems.Select(t =>
                {
                    var resourceSubtypePayload = JsonSerializer.Deserialize<ResourceSubTypePayload>(t.ResourceSubType.Payload);

                    var response = ResourceMapper.ProjectCompiled.Invoke(t);

                    response.ResourceType = resourceTypes.First(t => t.Code == resourceSubtypePayload.ResourceType);

                    return response;
                })
                .ToList();

            var model = new PagedListModel<ResourceResponse>
            {
                Items = items,
                Total = total,
                Page = pagingParams.Page,
                PageSize = pagingParams.PageSize
            };

            return model;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResourceResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var data = await service.Get()
                .Where(t => t.Id == id)
                .FirstAsync(map: ResourceMapper.IntermediateProject(), cancellationToken: cancellationToken);

            if (data == null)
                return NotFound();

            var resourceSubtypePayload = JsonSerializer.Deserialize<ResourceSubTypePayload>(data.ResourceSubType.Payload);

            data.ResourceType = await classifierService
                .Get()
                .Where(t => t.Type == ClassifierTypes.ResourceType && t.Code == resourceSubtypePayload.ResourceType)
                .FirstAsync(cancellationToken: cancellationToken);

            return ResourceMapper.ProjectCompiled.Invoke(data);
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.ResourceEdit)]
        public async Task<ActionResult> Update(Guid id, ResourceUpdateRequest model, CancellationToken cancellationToken = default)
        {
            await service.UpdateAsync(id, ResourceMapper.Map(model, new ResourceUpdateDto()), cancellationToken);

            return NoContent();
        }
    }
}
