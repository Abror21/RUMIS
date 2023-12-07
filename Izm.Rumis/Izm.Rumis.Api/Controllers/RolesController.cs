using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Izm.Rumis.Application.Contracts;

namespace Izm.Rumis.Api.Controllers
{
    [PermissionAuthorize(Permission.RoleView)]
    public sealed class RolesController : ApiController
    {
        private readonly IRoleService roleService;

        public RolesController(IRoleService roleService)
        {
            this.roleService = roleService;
        }

        [HttpPost]
        [PermissionAuthorize(Permission.RoleEdit)]
        public async Task<ActionResult<int>> Create(RoleEditRequest model, CancellationToken cancellationToken = default)
        {
            return await roleService.CreateAsync(RoleMapper.Map(model, new RoleEditDto()), cancellationToken);
        }

        [HttpGet]
        public async Task<ActionResult<PagedListModel<RoleResponse>>> Get([FromQuery] PagingRequest paging = null, CancellationToken cancellationToken = default)
        {
            var pagingParams = new PagingParams<Role>(paging)
               .AddDefaultSorting(t => t.Name, SortDirection.Desc)
               .AddSorting("Code", t => t.Code)
               .AddSorting("Name", t => t.Name)
               .AddSorting("Created", t => t.Created)
               .AddSorting("Modified", t => t.Modified);

            var query = roleService.Get();

            var total = await query.CountAsync(cancellationToken);

            var data = await roleService.Get()
                .Paging(pagingParams)
                .ListAsync(map: RoleMapper.Project(), cancellationToken: cancellationToken);

            var model = new PagedListModel<RoleResponse>
            {
                Items = data,
                Total = total,
                Page = pagingParams.Page,
                PageSize = pagingParams.PageSize
            };

            return model;
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(Permission.RoleEdit)]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            await roleService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.RoleEdit)]
        public async Task<ActionResult> Update(int id, RoleEditRequest model, CancellationToken cancellationToken = default)
        {
            await roleService.UpdateAsync(id, RoleMapper.Map(model, new RoleEditDto()), cancellationToken);

            return NoContent();
        }
    }
}
