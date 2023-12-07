using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Mappers;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public class ClassifierService : IClassifierService
    {
        private readonly IAppDbContext db;
        private readonly IClassifierValidator validator;
        private readonly IAuthorizationService authorizationService;
        private readonly ICurrentUserProfileService currentUserProfile;

        public ClassifierService(
            IAppDbContext db,
            IClassifierValidator validator,
            IAuthorizationService authorizationService,
            ICurrentUserProfileService currentUserProfile)
        {
            this.db = db;
            this.validator = validator;
            this.authorizationService = authorizationService;
            this.currentUserProfile = currentUserProfile;
        }

        /// <inheritdoc />
        public SetQuery<Classifier> Get()
        {
            var query = db.Classifiers.AsNoTracking();

            switch (currentUserProfile.Type)
            {
                case UserProfileType.Supervisor:
                    query = query.Where(t => ClassifierTypes.RequiredStatuses.Contains(t.Type)
                                || t.PermissionType == UserProfileType.Country
                                || t.SupervisorId == currentUserProfile.SupervisorId
                                || t.EducationalInstitution.SupervisorId == currentUserProfile.SupervisorId
                                );
                    break;

                case UserProfileType.EducationalInstitution:
                    query = query.Where(t => ClassifierTypes.RequiredStatuses.Contains(t.Type)
                                || t.PermissionType == UserProfileType.Country
                                || t.Supervisor.EducationalInstitutions.Any(t => t.Id == currentUserProfile.EducationalInstitutionId)
                                || t.EducationalInstitutionId == currentUserProfile.EducationalInstitutionId
                                );
                    break;

                case UserProfileType.Country:
                default:
                    break;
            }

            return new SetQuery<Classifier>(query);
        }

        /// <inheritdoc />
        /// <exception cref="ValidationException"></exception>
        public async Task<Guid> CreateAsync(ClassifierCreateDto item, CancellationToken cancellationToken = default)
        {
            authorizationService.Authorize(item);

            var entity = new Classifier
            {
                Id = Guid.NewGuid(),
                Type = Utility.SanitizeCode(item.Type),
                PermissionType = item.PermissionType
            };

            if (!entity.IsRequired)
            {
                entity.Code = Utility.SanitizeCode(item.Code);
                entity.IsDisabled = item.IsDisabled;
                entity.ActiveFrom = item.ActiveFrom;
                entity.ActiveTo = item.ActiveTo;
            }

            if (entity.PermissionType == UserProfileType.Supervisor)
                entity.SupervisorId = item.SupervisorId;

            if (entity.PermissionType == UserProfileType.EducationalInstitution)
                entity.EducationalInstitutionId = item.EducationalInstitutionId;

            ClassifierMapper.Map(item, entity);

            await validator.ValidateAsync(entity, cancellationToken);

            await db.Classifiers.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        /// <inheritdoc />
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="ValidationException"></exception>
        public async Task UpdateAsync(Guid id, ClassifierUpdateDto item, CancellationToken cancellationToken = default)
        {
            authorizationService.Authorize(item);

            var entity = await db.Classifiers.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity);

            if (!entity.IsRequired)
            {
                entity.Code = Utility.SanitizeCode(item.Code);
                entity.IsDisabled = item.IsDisabled;
                entity.ActiveFrom = item.ActiveFrom;
                entity.ActiveTo = item.ActiveTo;
            }

            if (entity.PermissionType == UserProfileType.Supervisor)
                entity.SupervisorId = item.SupervisorId;

            if (entity.PermissionType == UserProfileType.EducationalInstitution)
                entity.EducationalInstitutionId = item.EducationalInstitutionId;

            ClassifierMapper.Map(item, entity);

            await validator.ValidateAsync(entity, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="ValidationException"></exception>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.Classifiers.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            if (entity.IsRequired)
                throw new InvalidOperationException(Error.CannotDeleteRequired);

            if (entity.Type == ClassifierTypes.ClassifierType)
            {
                var hasChildren = await db.Classifiers.AnyAsync(t => t.Type == entity.Code, cancellationToken);

                if (hasChildren)
                    throw new InvalidOperationException(Error.CannotDeleteTypeNotEmpty);
            }

            db.Classifiers.Remove(entity);

            await db.SaveChangesAsync(cancellationToken);
        }

        public static class Error
        {
            public const string CannotDeleteRequired = "classifier.cannotDeleteRequired";
            public const string CannotDeleteTypeNotEmpty = "classifier.cannotDeleteTypeNotEmpty";
        }
    }
}
