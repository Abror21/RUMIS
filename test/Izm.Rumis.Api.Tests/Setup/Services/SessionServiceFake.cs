using Izm.Rumis.Application.Common;
using Izm.Rumis.Infrastructure.Sessions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal sealed class SessionServiceFake : ISessionService
    {
        public Guid? DeleteCalledWith { get; set; } = null;
        public CreateCalledWith CreateCalledWith { get; set; } = null;

        public Task CreateAsync(Guid id, DateTime? created = null, CancellationToken cancellationToken = default)
        {
            CreateCalledWith = new CreateCalledWith(id, created);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            DeleteCalledWith = id;

            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public SetQuery<Session> Get()
        {
            throw new NotImplementedException();
        }
    }

    public record CreateCalledWith(Guid Id, DateTime? Created);
}
