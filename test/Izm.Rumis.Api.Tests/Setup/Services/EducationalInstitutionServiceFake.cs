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
    public sealed class EducationalInstitutionServiceFake : IEducationalInstitutionService
    {
        public IQueryable<EducationalInstitution> EducationalInstitutions { get; set; } = new TestAsyncEnumerable<EducationalInstitution>(new List<EducationalInstitution>());

        public Task<int> CreateAsync(EducationalInstitutionCreateDto item, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public SetQuery<EducationalInstitution> Get()
        {
            return new SetQuery<EducationalInstitution>(EducationalInstitutions);
        }

        public Task UpdateAsync(int id, EducationalInstitutionUpdateDto item, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
