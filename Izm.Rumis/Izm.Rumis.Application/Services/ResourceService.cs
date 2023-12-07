using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Helpers;
using Izm.Rumis.Application.Mappers;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public sealed class ResourceService : IResourceService
    {
        private readonly IAppDbContext db;
        private readonly ISequenceService sequenceService;
        private readonly ICurrentUserProfileService currentUserProfile;
        private readonly IAuthorizationService authorizationService;
        private readonly IResourceValidator validator;

        public ResourceService(
            IAppDbContext db,
            ICurrentUserProfileService currentUserProfile,
            IAuthorizationService authorizationService,
            ISequenceService sequenceService,
            IResourceValidator validator)
        {
            this.db = db;
            this.currentUserProfile = currentUserProfile;
            this.authorizationService = authorizationService;
            this.sequenceService = sequenceService;
            this.validator = validator;
        }

        /// <inheritdoc/>
        public SetQuery<Resource> Get()
        {
            var query = db.Resources.AsNoTracking();

            switch (currentUserProfile.Type)
            {
                case UserProfileType.Supervisor:
                    query = query.Where(t => t.EducationalInstitution.SupervisorId == currentUserProfile.SupervisorId);
                    break;

                case UserProfileType.EducationalInstitution:
                    query = query.Where(t => t.EducationalInstitutionId == currentUserProfile.EducationalInstitutionId);
                    break;

                case UserProfileType.Country:
                default:
                    break;
            }

            return new SetQuery<Resource>(query);
        }

        /// <inheritdoc/>
        public async Task<Guid> CreateAsync(ResourceCreateDto item, CancellationToken cancellationToken = default)
        {
            authorizationService.Authorize(item.EducationalInstitutionId);

            await validator.ValidateAsync(item, cancellationToken);

            var educationalInstitutionCode = await db.EducationalInstitutions
              .Where(t => t.Id == item.EducationalInstitutionId)
              .Select(t => t.Code)
              .FirstAsync(cancellationToken);

            var serialNumberWithinInsitution = sequenceService.GetByKey(NumberingPatternHelper.ResourceKeyFormat(educationalInstitutionCode));

            var entity = ResourceMapper.Map(item, new Resource());

            entity.ResourceNumber = NumberingPatternHelper.ResourceNumberFormat(educationalInstitutionCode, serialNumberWithinInsitution);

            await db.Resources.AddAsync(entity, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Guid id, ResourceUpdateDto item, CancellationToken cancellationToken = default)
        {
            var entity = await db.Resources
                        .Include(t => t.ResourceParameters)
                        .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity.EducationalInstitutionId);

            ResourceMapper.Map(item, entity);

            //Update ResourceParameters

            entity.ResourceParameters = entity.ResourceParameters.Where(t => item.ResourceParameters.Any(d => d.Id == t.Id)).ToList();

            foreach (var entityResourceParameter in entity.ResourceParameters)
            {
                var itemResourceParameter = item.ResourceParameters.FirstOrDefault(t => t.Id == entityResourceParameter.Id);
                entityResourceParameter.Value = itemResourceParameter.Value;
                entityResourceParameter.ParameterId = itemResourceParameter.ParameterId;
            }

            var resourceParamteresToAdd = item.ResourceParameters.Where(t => t.Id == null).ToArray();

            foreach (var resourceParameter in resourceParamteresToAdd)
                entity.ResourceParameters.Add(new Domain.Entities.ResourceParameter
                {
                    Value = resourceParameter.Value,
                    ParameterId = resourceParameter.ParameterId
                });

            await validator.ValidateAsync(entity, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
