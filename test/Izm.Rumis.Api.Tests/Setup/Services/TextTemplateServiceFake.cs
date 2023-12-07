using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal class TextTemplateServiceFake : ITextTemplateService
    {
        public IQueryable<TextTemplate> TextTemplates { get; set; } = new TestAsyncEnumerable<TextTemplate>(new List<TextTemplate>());

        public int CreateResult { get; set; } = 1;

        public Task<int> CreateAsync(TextTemplateEditDto item, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResult);
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public SetQuery<TextTemplate> Get()
        {
            return new SetQuery<TextTemplate>(TextTemplates);
        }

        public Task UpdateAsync(int id, TextTemplateEditDto item, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
