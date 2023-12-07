using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Services;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    //[IdentityAuthorize(IdentityPermissions.DocumentTemplateView)]
    [PermissionAuthorize(Permission.DocumentTemplateView)]
    public class DocumentTemplatesController : ApiController
    {
        private readonly IDocumentTemplateService service;
        private readonly IClassifierService classifierService;

        public DocumentTemplatesController(IDocumentTemplateService service,
            IClassifierService classifierService)
        {
            this.service = service;
            this.classifierService = classifierService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedListModel<DocumentTemplateModel>>> Get([FromQuery] DocumentTemplateFilterRequest filter = null, [FromQuery] PagingRequest paging = null, CancellationToken cancellationToken = default)
        {
            var pagingParams = new PagingParams<DocumentTemplate>(paging)
               .AddDefaultSorting(t => t.Created, SortDirection.Desc)
               .AddSorting("Title", t => t.Title)
               .AddSorting("Code", t => t.Code)
               .AddSorting("Hyperlink", t => t.Hyperlink)
               .AddSorting("ResourceType", t => t.ResourceType)
               .AddSorting("ValidFrom", t => t.ValidFrom)
               .AddSorting("ValidTo", t => t.ValidTo)
               .AddSorting("Supervisor", t => t.EducationalInstitution.Supervisor.Name);

            var query = service.Get().Filter(filter);

            var total = await query.CountAsync(cancellationToken);

            var documentTemplateTypes = await classifierService.Get()
              .Where(t => t.Type == ClassifierTypes.DocumentType)
              .ListAsync(map: ApplicationResourceListItem.ClassifierData.Project(), cancellationToken: cancellationToken);


            var intermediateItems = await query
                .Paging(pagingParams)
                .ListAsync(map: DocumentTemplateMapper.IntermediateProject(), cancellationToken: cancellationToken);

            var items = intermediateItems.Select(t =>
            {

                var response = DocumentTemplateMapper.ProjectCompiled.Invoke(t);

                var documentType = documentTemplateTypes.FirstOrDefault(d => d.Code == t.Code);

                response.DocumentType = documentType == null ? null : new DocumentTemplateModel.ClassifierData
                {
                    Id = documentType.Id,
                    Code = documentType.Code,
                    Value = documentType.Value
                };
                return response;
            })
            .ToList();

            var model = new PagedListModel<DocumentTemplateModel>
            {
                Items = items,
                Total = total,
                Page = pagingParams.Page,
                PageSize = pagingParams.PageSize
            };

            return model;
        }

        [HttpPost]
        [PermissionAuthorize(Permission.DocumentTemplateEdit)]
        public async Task<ActionResult<int>> Post([FromForm] DocumentTemplateCreateModel model, CancellationToken cancellationToken = default)
        {
            var result = await service.CreateAsync(new DocumentTemplateEditDto
            {
                Code = model.Code,
                Title = model.Title,
                ValidFrom = Mapper.MapDateOnly(model.ValidFrom),
                ValidTo = Mapper.MapDateOnly(model.ValidTo),
                Hyperlink = model.Hyperlink,
                ResourceTypeId = model.ResourceTypeId,
                File = model.File == null ? null : Mapper.MapFile(model.File, new FileDto()),
                SupervisorId = model.SupervisorId,
                EducationalInstitutionId = model.EducationalInstitutionId,
                PermissionType = model.PermissionType
            }, cancellationToken);

            return result;
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.DocumentTemplateEdit)]
        public async Task<IActionResult> Put(int id, [FromForm] DocumentTemplateUpdateModel model, CancellationToken cancellationToken = default)
        {
            var dto = new DocumentTemplateEditDto
            {
                Code = model.Code,
                Title = model.Title,
                ValidFrom = Mapper.MapDateOnly(model.ValidFrom),
                ValidTo = Mapper.MapDateOnly(model.ValidTo),
                Hyperlink = model.Hyperlink,
                ResourceTypeId = model.ResourceTypeId,
                File = model.File == null ? null : Mapper.MapFile(model.File, new FileDto()),
                SupervisorId = model.SupervisorId,
                EducationalInstitutionId = model.EducationalInstitutionId,
                PermissionType = model.PermissionType
            };

            await service.UpdateAsync(id, dto, cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(Permission.DocumentTemplateEdit)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            await service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id}/sample")]
        public Task<string> Sample(int id, CancellationToken cancellationToken = default)
        {
            return service.GetSampleAsync(id, cancellationToken);
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download([FromServices] IFileManager fileManager, int id, CancellationToken cancellationToken = default)
        {
            var template = await service.Get().Where(t => t.Id == id).FirstAsync(t => new
            {
                t.FileId
            }, cancellationToken);

            return template == null ? NotFound() : await fileManager.Download(template.FileId);
        }
    }
}
