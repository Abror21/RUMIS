using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    public sealed class ResourceServiceFake : IResourceService
    {
        public IQueryable<Resource> Resources { get; set; } = new TestAsyncEnumerable<Resource>(new List<Resource>());

        public SetQuery<Resource> Get()
        {
            return new SetQuery<Resource>(Resources);
        }

        public Task<Guid> CreateAsync(ResourceCreateDto item, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Guid id, ResourceUpdateDto item, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
