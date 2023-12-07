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
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    [PermissionAuthorize(Permission.SupervisorView)]
    public class SupervisorsController : ApiController
    {
        private readonly ISupervisorService supervisorService;

        public SupervisorsController(ISupervisorService supervisorService)
        {
            this.supervisorService = supervisorService;
        }

        [HttpPost]
        [PermissionAuthorize(Permission.SupervisorEdit)]
        public async Task<ActionResult<int>> Create(SupervisorCreateRequest model, CancellationToken cancellationToken = default)
        {
            return await supervisorService.CreateAsync(SupervisorMapper.Map(model, new SupervisorCreateDto()), cancellationToken);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupervisorResponse>>> Get(CancellationToken cancellationToken = default)
        {
            var data = await supervisorService.Get()
                .ListAsync(map: SupervisorMapper.Project(), cancellationToken: cancellationToken);

            return data.ToArray();
        }

        [HttpGet("{id}")]
        public SupervisorByIdResponse GetById(int id, CancellationToken cancellationToken = default)
        {
            return supervisorService.Get()
                .Where(t => t.Id == id)
                .First(SupervisorMapper.ProjectById());
        }

        [HttpGet("list")]
        public async Task<ActionResult<PagedListModel<SupervisorListItemResponse>>> GetList([FromQuery] PagingRequest paging = null, [FromQuery] SupervisorListItemFilterRequest filter = null, CancellationToken cancellationToken = default)
        {
            var pagingParams = new PagingParams<Supervisor>(paging)
                .SetMaxPageSize(100)
                .AddDefaultSorting(t => t.Name)
                .AddSorting("Name", t => t.Name)
                .AddSorting("Status", t => t.IsActive)
                .AddSorting("ActiveEducationalInstitutions", t => t.EducationalInstitutions
                                                                   .Where(e => e.Status.Code == EducationalInstitutionStatus.Active)
                                                                   .Count());

            var query = supervisorService.Get()
                .Filter(filter);

            var total = await query.CountAsync(cancellationToken);

            var data = await query.Paging(pagingParams)
                .ListAsync(map: SupervisorMapper.ProjectSupervisorListItem(), cancellationToken: cancellationToken);

            var model = new PagedListModel<SupervisorListItemResponse>
            {
                Items = data,
                Page = pagingParams.Page,
                PageSize = pagingParams.PageSize,
                Total = total
            };

            return model;
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(Permission.SupervisorEdit)]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            await supervisorService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.SupervisorEdit)]
        public async Task<ActionResult> Update(int id, SupervisorUpdateRequest model, CancellationToken cancellationToken = default)
        {
            await supervisorService.UpdateAsync(id, SupervisorMapper.Map(model, new SupervisorUpdateDto()), cancellationToken);

            return NoContent();
        }
    }
}
