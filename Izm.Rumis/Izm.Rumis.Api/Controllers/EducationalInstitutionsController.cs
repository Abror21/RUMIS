using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    [PermissionAuthorize(Permission.EducationalInstitutionView)]
    public class EducationalInstitutionsController : ApiController
    {
        private readonly IEducationalInstitutionService EducationalInstitutionService;

        public EducationalInstitutionsController(IEducationalInstitutionService EducationalInstitutionService)
        {
            this.EducationalInstitutionService = EducationalInstitutionService;
        }

        [HttpPost]
        [PermissionAuthorize(Permission.EducationalInstitutionEdit)]
        public async Task<ActionResult<int>> Create(EducationalInstitutionCreateRequest model, CancellationToken cancellationToken = default)
        {
            return await EducationalInstitutionService.CreateAsync(EducationalInstitutionMapper.Map(model, new EducationalInstitutionCreateDto()), cancellationToken);
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(Permission.EducationalInstitutionEdit)]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            await EducationalInstitutionService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }

        [HttpGet("list")]
        public async Task<ActionResult<PagedListModel<EducationalInstitutionListItemResponse>>> GetList([FromQuery] PagingRequest paging = null, [FromQuery] EducationalInstitutionListItemFilterRequest filter = null, CancellationToken cancellationToken = default)
        {
            var pagingParams = new PagingParams<EducationalInstitution>(paging)
                .SetMaxPageSize(100)
                .AddDefaultSorting(t => t.Name)
                .AddSorting("Name", t => t.Name)
                .AddSorting("Status", t => t.Status)
                .AddSorting("SupervisorName", t => t.Supervisor.Name);

            var query = EducationalInstitutionService.Get()
                .Filter(filter);

            var total = await query.CountAsync(cancellationToken);

            var data = await query.Paging(pagingParams)
                .ListAsync(map: EducationalInstitutionMapper.ProjectEducationalInstitutionListItem(), cancellationToken: cancellationToken);

            var model = new PagedListModel<EducationalInstitutionListItemResponse>
            {
                Items = data,
                Page = pagingParams.Page,
                PageSize = pagingParams.PageSize,
                Total = total
            };

            return model;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EducationalInstitutionResponse>>> Get(CancellationToken cancellationToken = default)
        {
            var data = await EducationalInstitutionService.Get()
                .ListAsync(map: EducationalInstitutionMapper.Project(), cancellationToken: cancellationToken);

            return data.ToArray();
        }

        [HttpGet("{id}")]
        public Task<EducationalInstitutionResponse> GetById(int id, CancellationToken cancellationToken = default)
        {
            return EducationalInstitutionService.Get()
                .Where(t => t.Id == id)
                .FirstAsync(EducationalInstitutionMapper.Project(), cancellationToken);
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.EducationalInstitutionEdit)]
        public async Task<ActionResult> Update(int id, EducationalInstitutionUpdateRequest model, CancellationToken cancellationToken = default)
        {
            await EducationalInstitutionService.UpdateAsync(id, EducationalInstitutionMapper.Map(model, new EducationalInstitutionUpdateDto()), cancellationToken);

            return NoContent();
        }
    }
}
