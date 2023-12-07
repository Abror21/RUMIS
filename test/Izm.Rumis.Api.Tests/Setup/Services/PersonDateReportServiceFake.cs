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
    internal sealed class PersonDateReportServiceFake : IPersonDataReportService
    {
        public IQueryable<GdprAudit> GenerateData { get; set; } = new TestAsyncEnumerable<GdprAudit>(new List<GdprAudit>());
        public PersonDataReportGenerateDto GenerateAsyncCalledWith { get; set; } = null;

        public Task<SetQuery<GdprAudit>> GenerateAsync(PersonDataReportGenerateDto item, CancellationToken cancellationToken = default)
        {
            GenerateAsyncCalledWith = item;

            return Task.FromResult(new SetQuery<GdprAudit>(GenerateData));
        }
    }
}
