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
    public sealed class SupervisorServiceFake : ISupervisorService
    {
        public IQueryable<Supervisor> Supervisors { get; set; } = new TestAsyncEnumerable<Supervisor>(new List<Supervisor>());

        public Task<int> CreateAsync(SupervisorCreateDto item, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public SetQuery<Supervisor> Get()
        {
            return new SetQuery<Supervisor>(Supervisors);
        }

        public Task UpdateAsync(int id, SupervisorUpdateDto item, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
