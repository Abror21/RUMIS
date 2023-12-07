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
    public class UserProfileServiceFake : IUserProfileService
    {
        public Guid? ActivatedId { get; set; } = null;
        public bool GetCurrentUserProfilesCalled { get; set; } = false;
        public IQueryable<UserProfile> CurrentUserProfiles { get; set; } = new TestAsyncEnumerable<UserProfile>(new List<UserProfile>());
        public IQueryable<UserProfile> UserProfiles { get; set; } = new TestAsyncEnumerable<UserProfile>(new List<UserProfile>());

        public Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ActivatedId = id;

            return Task.CompletedTask;
        }

        public Task<Guid> CreateAsync(UserProfileEditDto item, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Guid.NewGuid());
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public SetQuery<UserProfile> Get()
        {
            return new SetQuery<UserProfile>(UserProfiles);
        }

        public SetQuery<UserProfile> GetCurrentUserProfiles()
        {
            GetCurrentUserProfilesCalled = true;

            return new SetQuery<UserProfile>(CurrentUserProfiles);
        }

        public Task UpdateAsync(Guid id, UserProfileEditDto item, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
