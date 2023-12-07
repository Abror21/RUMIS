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
    internal class DocumentTemplateServiceFake : IDocumentTemplateService
    {
        public IQueryable<DocumentTemplate> DocumentTemplates { get; set; } = new TestAsyncEnumerable<DocumentTemplate>(new List<DocumentTemplate>());

        public string DocumentTemplateSample { get; set; }

        public int CreateResult { get; set; } = 1;
        public int? GetByEducationalInstitutionCalledWith { get; set; } = null;
        public int? GetSampleAsyncCalledWith { get; set; } = null;

        public Task<int> CreateAsync(DocumentTemplateEditDto item, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResult);
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public SetQuery<DocumentTemplate> Get()
        {
            return new SetQuery<DocumentTemplate>(DocumentTemplates);
        }

        public SetQuery<DocumentTemplate> GetByEducationalInstitution(int eduInstId)
        {
            GetByEducationalInstitutionCalledWith = eduInstId;

            return new SetQuery<DocumentTemplate>(DocumentTemplates);
        }

        public Task<string> GetSampleAsync(int id, CancellationToken cancellationToken = default)
        {
            GetSampleAsyncCalledWith = id;

            return Task.FromResult(DocumentTemplateSample);
        }

        public Task UpdateAsync(int id, DocumentTemplateEditDto item, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
