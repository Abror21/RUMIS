using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Mappers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public sealed class UserProfileService : IUserProfileService
    {
        private readonly IAppDbContext db;
        private readonly IAuthorizationService authorizationService;
        private readonly ICurrentUserService currentUser;
        private readonly ICurrentUserProfileService currentUserProfile;
        private readonly IGdprAuditService gdprAuditService;

        public UserProfileService(
            IAppDbContext db,
            IAuthorizationService authorizationService,
            ICurrentUserService currentUserService,
            ICurrentUserProfileService currentUserProfile,
            IGdprAuditService gdprAuditService)
        {
            this.db = db;
            this.authorizationService = authorizationService;
            this.currentUserProfile = currentUserProfile;
            this.currentUser = currentUserService;
            this.gdprAuditService = gdprAuditService;
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.UserProfiles.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            if (entity.IsLoggedIn)
                return;

            entity.IsLoggedIn = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Guid> CreateAsync(UserProfileEditDto item, CancellationToken cancellationToken = default)
        {
            authorizationService.Authorize((IAuthorizedResourceCreateDto)item);

            var entity = UserProfile.Create();

            entity.SetDisabled(item.IsDisabled);
            entity.SetExpiration(item.Expires);

            UserProfileMapper.Map(item, entity);

            entity.SetAccessLevel(new AccessLevel
            {
                EducationalInstitutionId = item.EducationalInstitutionId,
                SupervisorId = item.SupervisorId,
                Type = item.PermissionType
            });

            entity.SetRoles(
                await db.Roles
                    .Where(t => item.RoleIds.Contains(t.Id))
                    .ToArrayAsync(cancellationToken)
                );

            await db.UserProfiles.AddAsync(entity, cancellationToken);

            if (!string.IsNullOrEmpty(item.PhoneNumber) || !string.IsNullOrEmpty(item.Email))
            {
                var personTechnicalIdWrapper = await db.PersonTechnicals
                    .Where(t => t.UserId == entity.UserId)
                    .Select(t => new { t.Id })
                    .FirstOrDefaultAsync(cancellationToken);

                if (personTechnicalIdWrapper != null)
                    await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForCreateOperation(entity.Id, personTechnicalIdWrapper.Id, item));
            }

            await db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.UserProfiles
                .Include(t => t.Roles)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            if (entity.IsLoggedIn)
                throw new InvalidOperationException(Error.CannotDeleteActivated);

            entity.ClearRoles();

            db.UserProfiles.Remove(entity);

            if (!string.IsNullOrEmpty(entity.PhoneNumber) || !string.IsNullOrEmpty(entity.Email))
            {
                var personTechnicalIdWrapper = await db.PersonTechnicals
                    .Where(t => t.UserId == entity.UserId)
                    .Select(t => new { t.Id })
                    .FirstOrDefaultAsync(cancellationToken);

                if (personTechnicalIdWrapper != null)
                    await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForDeleteOperation(entity, personTechnicalIdWrapper.Id));
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public SetQuery<UserProfile> Get()
        {
            var query = db.UserProfiles.AsNoTracking();

            switch (currentUserProfile.Type)
            {
                case UserProfileType.Supervisor:
                    query = query.Where(t => t.SupervisorId == currentUserProfile.SupervisorId
                                || t.EducationalInstitution.SupervisorId == currentUserProfile.SupervisorId);
                    break;

                case UserProfileType.EducationalInstitution:
                    query = query.Where(t => t.EducationalInstitutionId == currentUserProfile.EducationalInstitutionId);
                    break;

                case UserProfileType.Country:
                default:
                    break;
            }

            return new SetQuery<UserProfile>(query);
        }

        /// <inheritdoc/>
        public SetQuery<UserProfile> GetCurrentUserProfiles()
        {
            var query = db.UserProfiles.AsNoTracking()
                .Where(t => t.UserId == currentUser.Id);

            return new SetQuery<UserProfile>(query);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task UpdateAsync(Guid id, UserProfileEditDto item, CancellationToken cancellationToken = default)
        {
            authorizationService.Authorize((IAuthorizedResourceEditDto)item);

            var entity = await db.UserProfiles
                .Include(t => t.Roles)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity);

            var rolesListTask = db.Roles
                .Where(t => item.RoleIds.Contains(t.Id))
                .ToArrayAsync(cancellationToken);

            entity.SetDisabled(item.IsDisabled);
            entity.SetExpiration(item.Expires);

            UserProfileMapper.Map(item, entity);

            entity.SetAccessLevel(new AccessLevel
            {
                EducationalInstitutionId = item.EducationalInstitutionId,
                SupervisorId = item.SupervisorId,
                Type = item.PermissionType
            });

            entity.SetRoles(await rolesListTask);

            var personTechnicalIdWrapper = await db.PersonTechnicals
                .Where(t => t.UserId == entity.UserId)
                .Select(t => new { t.Id })
                .FirstOrDefaultAsync(cancellationToken);

            if (personTechnicalIdWrapper != null)
                await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForUpdateOperation(entity.Id, personTechnicalIdWrapper.Id, item));

            await db.SaveChangesAsync(cancellationToken);
        }

        public static class Error
        {
            public const string CannotDeleteActivated = "userProfile.cannotDeleteActivated";
        }

        public static class GdprAuditHelper
        {
            public static GdprAuditTraceDto GenerateTraceForCreateOperation(Guid userProfileId, Guid dataOwnerId, UserProfileEditDto item)
            {
                var data = new List<PersonDataProperty>();

                if (!string.IsNullOrEmpty(item.Email))
                    data.Add(new PersonDataProperty { Type = PersonDataType.Contact, Value = item.Email });

                if (!string.IsNullOrEmpty(item.PhoneNumber))
                    data.Add(new PersonDataProperty { Type = PersonDataType.Contact, Value = item.PhoneNumber });

                return new GdprAuditTraceDto
                {
                    Action = "userProfile.create",
                    ActionData = JsonSerializer.Serialize(new { UserProfileId = userProfileId }),
                    DataOwnerId = dataOwnerId,
                    EducationalInstitutionId = item.EducationalInstitutionId,
                    DataOwnerPrivatePersonalIdentifier = null,
                    Data = data
                };
            }

            public static GdprAuditTraceDto GenerateTraceForDeleteOperation(UserProfile entity, Guid dataOwnerId)
            {
                var data = new List<PersonDataProperty>();

                if (!string.IsNullOrEmpty(entity.Email))
                    data.Add(new PersonDataProperty { Type = PersonDataType.Contact, Value = entity.Email });

                if (!string.IsNullOrEmpty(entity.PhoneNumber))
                    data.Add(new PersonDataProperty { Type = PersonDataType.Contact, Value = entity.PhoneNumber });

                return new GdprAuditTraceDto
                {
                    Action = "userProfile.delete",
                    ActionData = JsonSerializer.Serialize(new { UserProfileId = entity.Id }),
                    DataOwnerId = dataOwnerId,
                    EducationalInstitutionId = entity.EducationalInstitutionId,
                    DataOwnerPrivatePersonalIdentifier = null,
                    Data = data
                };
            }

            public static GdprAuditTraceDto GenerateTraceForUpdateOperation(Guid userProfileId, Guid dataOwnerId, UserProfileEditDto item)
            {
                var data = new List<PersonDataProperty>
                {
                    new PersonDataProperty { Type = PersonDataType.Contact, Value = item.Email },
                    new PersonDataProperty { Type = PersonDataType.Contact, Value = item.PhoneNumber }
                };

                return new GdprAuditTraceDto
                {
                    Action = "userProfile.update",
                    ActionData = JsonSerializer.Serialize(new { UserProfileId = userProfileId }),
                    DataOwnerId = dataOwnerId,
                    EducationalInstitutionId = item.EducationalInstitutionId,
                    DataOwnerPrivatePersonalIdentifier = null,
                    Data = data
                };
            }
        }
    }
}
