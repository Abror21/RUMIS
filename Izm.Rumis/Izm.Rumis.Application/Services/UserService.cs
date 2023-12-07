using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public sealed class UserService : IUserService
    {
        private readonly IAppDbContext db;
        private readonly ICurrentUserProfileService currentUserProfile;

        public UserService(
            IAppDbContext db,
            ICurrentUserProfileService currentUserProfile)
        {
            this.db = db;
            this.currentUserProfile = currentUserProfile;
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.Users
                .Include(t => t.PersonTechnical)
                .Include(t => t.Profiles)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            if (entity.Profiles.Any(t => t.IsLoggedIn))
                throw new InvalidOperationException(Error.CannotDeleteActivated);

            if (entity.PersonTechnical != null)
                entity.PersonTechnical.UserId = null;

            foreach (var profile in entity.Profiles)
                profile.ClearRoles();

            db.Users.Remove(entity);

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public SetQuery<User> GetPersons()
        {
            var query = db.Users.AsNoTracking()
                .Where(t => t.PersonTechnical != null);

            switch (currentUserProfile.Type)
            {
                case UserProfileType.Supervisor:
                    query = query.Where(t => t.Profiles.Any(p =>
                                p.SupervisorId == currentUserProfile.SupervisorId
                                || p.EducationalInstitution.SupervisorId == currentUserProfile.SupervisorId));
                    break;

                case UserProfileType.EducationalInstitution:
                    query = query.Where(t => t.Profiles.Any(p =>
                                p.EducationalInstitutionId == currentUserProfile.EducationalInstitutionId));
                    break;

                case UserProfileType.Country:
                default:
                    break;
            }

            return new SetQuery<User>(query);
        }

        public static class Error
        {
            public const string CannotDeleteActivated = "user.cannotDeleteActivated";
        }
    }
}
