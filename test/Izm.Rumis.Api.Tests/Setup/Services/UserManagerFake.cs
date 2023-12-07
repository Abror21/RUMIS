using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Infrastructure.Identity;
using Izm.Rumis.Infrastructure.Identity.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal class UserManagerFake : IUserManager
    {
        public IQueryable<IdentityUserLogin> UserLogins { get; set; } = new TestAsyncEnumerable<IdentityUserLogin>(new List<IdentityUserLogin>());
        public IQueryable<IdentityUser> Users { get; set; } = new TestAsyncEnumerable<IdentityUser>(new List<IdentityUser>());

        public Guid? UserId { get; set; } = null;
        public Guid CreateResult { get; set; } = Guid.NewGuid();
        public Guid ResetPasswordResult { get; set; } = Guid.NewGuid();
        public bool VerifyPasswordResult { get; set; } = true;

        public Task AddRefreshTokenAsync(Guid userId, string token, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task ChangePassword(string username, string password)
        {
            return Task.CompletedTask;
        }

        public Task<Guid> CreateAsync(UserCreateDto item, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResult);
        }

        public SetQuery<IdentityUserLogin> GetLogins()
        {
            return new SetQuery<IdentityUserLogin>(UserLogins);
        }

        public SetQuery<IdentityUser> GetUsers()
        {
            return new SetQuery<IdentityUser>(Users);
        }

        public Task<string> MustResetPassword(string username, bool force = false)
        {
            throw new NotImplementedException();
        }

        public Task RemoveProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<Guid> ResetPassword(string secret, string password)
        {
            return Task.FromResult(ResetPasswordResult);
        }

        public Task SetRolesAsync(Guid profileId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> SetRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyPassword(string username, string password)
        {
            return Task.FromResult(VerifyPasswordResult);
        }

        public Task<bool> VerifyPassword(Guid userId, string password)
        {
            return Task.FromResult(VerifyPasswordResult);
        }

        public Task<Guid?> GetUserIdAsync(string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(UserId);
        }
    }
}
