using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    [PermissionAuthorize(Permission.ClassifierView)]
    public class ClassifiersController : ApiController
    {
        private readonly IClassifierService service;

        public ClassifiersController(IClassifierService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClassifierModel>>> Get(
            string type = null,
            string code = null,
            string group = null,
            bool includeDisabled = false,
            CancellationToken cancellationToken = default)
        {
            var query = service.Get();

            if (!includeDisabled)
                query.Where(t => !t.IsDisabled);

            if (!string.IsNullOrEmpty(type))
                query.Where(t => t.Type.ToLower() == type.ToLower());

            if (!string.IsNullOrEmpty(group))
            {
                var groups = (await service.Get()
                    .Where(t => t.Type == ClassifierTypes.ClassifierType && t.Payload != null)
                    .ListAsync(
                        map: t => new
                        {
                            t.Id,
                            t.Payload
                        }, cancellationToken: cancellationToken))
                    .Select(t => new
                    {
                        t.Id,
                        Group = ClassifierPayload.Parse<ClassifierTypePayload>(t.Payload)?.Group
                    })
                    .Where(t => string.Equals(t.Group, group, StringComparison.InvariantCultureIgnoreCase))
                    .Select(t => t.Id)
                    .ToArray();

                query.Where(t => groups.Contains(t.Id));
            }

            if (!string.IsNullOrEmpty(code))
                query.Where(t => t.Code.ToLower() == code.ToLower());

            var data = await query.OrderBy(t => t.SortOrder).ListAsync(map: ClassifierMapper.Project(), cancellationToken: cancellationToken);

            return data.ToList();
        }

        [HttpGet("getByType")]
        public async Task<ActionResult<IEnumerable<ClassifierModel>>> GetByTypeAsync([FromQuery] ClassifierTypeFilter filter, CancellationToken cancellationToken = default)
        {
            var query = service.Get();

            if (!filter.IncludeDisabled)
                query.Where(t => !t.IsDisabled);

            var types = filter.Types.Select(t => t.ToLower());

            var data = await query
                .Where(t => types.Contains(t.Type.ToLower()))
                .OrderBy(t => t.SortOrder)
                .ListAsync(map: ClassifierMapper.Project(), cancellationToken: cancellationToken);

            return data.ToList();
        }

        [HttpPost]
        [PermissionAuthorize(Permission.ClassifierEdit)]
        public async Task<ActionResult<Guid>> Post(ClassifierCreateModel model, CancellationToken cancellationToken = default)
        {
            var result = await service.CreateAsync(ClassifierMapper.Map(model, new ClassifierCreateDto()), cancellationToken);
            return result;
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.ClassifierEdit)]
        public async Task<IActionResult> Put(Guid id, ClassifierEditModel model, CancellationToken cancellationToken = default)
        {
            await service.UpdateAsync(id, ClassifierMapper.Map(model, new ClassifierUpdateDto()), cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(Permission.ClassifierEdit)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            await service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
