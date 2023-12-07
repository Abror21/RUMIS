using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    public class TextTemplatesController : ApiController
    {
        private readonly ITextTemplateService service;

        public TextTemplatesController(ITextTemplateService service)
        {
            this.service = service;
        }

        [HttpGet]
        [PermissionAuthorize(Permission.TextTemplateView)]
        public async Task<ActionResult<IEnumerable<TextTemplateModel>>> Get(string code = null, CancellationToken cancellationToken = default)
        {
            var query = service.Get();

            if (!string.IsNullOrEmpty(code))
                query = query.Where(t => t.Code == code);

            var data = await query.OrderBy(t => t.Code).ListAsync(
                map: t => new TextTemplateModel
                {
                    Code = t.Code,
                    Id = t.Id,
                    Title = t.Title,
                    Content = t.Content
                },
                cancellationToken: cancellationToken
                );

            return data.ToList();
        }

        [HttpGet("termsOfUse")]
        [AllowAnonymous]
        public async Task<ActionResult<TextTemplateModel>> TermsOfUse(CancellationToken cancellationToken = default)
        {
            var query = service.Get();

            query = query.Where(t => t.Code == TextTemplateCode.TermOfUse);

            var data = await query.FirstAsync(
                map: t => new TextTemplateModel
                {
                    Code = t.Code,
                    Id = t.Id,
                    Title = t.Title,
                    Content = t.Content
                },
                cancellationToken: cancellationToken
                );

            return data;
        }

        [HttpPost]
        [PermissionAuthorize(Permission.TextTemplateView)]
        public async Task<ActionResult<int>> Post(TextTemplateEditModel model, CancellationToken cancellationToken = default)
        {
            var result = await service.CreateAsync(new TextTemplateEditDto
            {
                Code = model.Code,
                Content = model.Content,
                Title = model.Title
            }, cancellationToken);

            return result;
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.TextTemplateView)]
        public async Task<IActionResult> Put(int id, TextTemplateEditModel model, CancellationToken cancellationToken = default)
        {
            await service.UpdateAsync(id, new TextTemplateEditDto
            {
                Code = model.Code,
                Content = model.Content,
                Title = model.Title
            }, cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(Permission.TextTemplateView)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            await service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
