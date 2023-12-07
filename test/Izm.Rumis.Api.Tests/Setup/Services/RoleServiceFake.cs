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
    public sealed class RoleServiceFake : IRoleService
    {
        public IQueryable<Role> Roles { get; set; } = new TestAsyncEnumerable<Role>(new List<Role>());

        public Task<int> CreateAsync(RoleEditDto item, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public SetQuery<Role> Get()
        {
            return new SetQuery<Role>(Roles);
        }

        public Task UpdateAsync(int id, RoleEditDto item, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
