using Izm.Rumis.Infrastructure.ResourceImport;
using Izm.Rumis.Infrastructure.ResourceImport.Dtos;
using Izm.Rumis.Infrastructure.ResourceImport.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    public sealed class ResourcesImportServiceFake : IResourceImportService
    {
        public Task<ResourcesImportResult> ImportAsync(ResourceImportDataDto item, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
