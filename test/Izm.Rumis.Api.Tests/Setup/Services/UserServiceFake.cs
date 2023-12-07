using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal sealed class UserServiceFake : IUserService
    {
        public Guid? Deleted { get; set; } = null;
        public IQueryable<User> Users { get; set; } = new TestAsyncEnumerable<User>(new List<User>());

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Deleted = id;

            return Task.CompletedTask;
        }

        public SetQuery<User> GetPersons()
        {
            return new SetQuery<User>(Users);
        }
    }
}
