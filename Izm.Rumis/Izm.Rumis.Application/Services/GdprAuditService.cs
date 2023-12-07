using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Mappers;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public sealed class GdprAuditService : IGdprAuditService
    {
        private readonly string cachePrefix = $"{nameof(GdprAuditService)}_";

        private readonly IAppDbContext db;
        private readonly ICurrentUserService currentUser;
        private readonly ICurrentUserProfileService currentUserProfile;
        private readonly IGdprAuditValidator validator;
        private readonly IDistributedCache distributedCache;

        public GdprAuditService(
            IAppDbContext db,
            IGdprAuditValidator validator,
            ICurrentUserService currentUser,
            ICurrentUserProfileService currentUserProfile,
            IDistributedCache distributedCache)
        {
            this.db = db;
            this.validator = validator;
            this.currentUser = currentUser;
            this.currentUserProfile = currentUserProfile;
            this.distributedCache = distributedCache;
        }

        /// <inheritdoc/>
        public async Task TraceAsync(GdprAuditTraceDto item, CancellationToken cancellationToken = default)
        {
            await HandleTraceAsync(item);

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task TraceRangeAsync(IEnumerable<GdprAuditTraceDto> items, CancellationToken cancellationToken = default)
        {
            foreach (var item in items)
                await HandleTraceAsync(item);

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Get cache key specific to this class.
        /// </summary>
        /// <param name="value">Cache value.</param>
        /// <returns>Complete cache key.</returns>
        private string GetCacheKey(string value) => $"{cachePrefix}{value}";

        /// <summary>
        /// Get private personal identifier for PersonTechnical.
        /// </summary>
        /// <param name="personTechnicalId">Person technical ID.</param>
        /// <returns>Private personal identifier.</returns>
        private async Task<string> GetPersonTechnicalPrivatePersonalIdentifierAsync(Guid personTechnicalId)
        {
            var cacheKey = GetCacheKey($"PersonTechnical_{personTechnicalId}_PrivatePersonalIdentifier");

            var privatePersonalIdentifier = await distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(privatePersonalIdentifier))
                return privatePersonalIdentifier;

            privatePersonalIdentifier = db.Persons
                .Where(person => person.PersonTechnicalId == personTechnicalId)
                .OrderBy(person => person.ActiveFrom)
                .Select(person => person.PrivatePersonalIdentifier)
                .Last();

            await distributedCache.SetStringAsync(cacheKey, privatePersonalIdentifier, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

            return privatePersonalIdentifier;
        }

        /// <summary>
        /// Validate and add to database a GDPR audit trace.
        /// </summary>
        /// <param name="item">Trace data.</param>
        private async Task HandleTraceAsync(GdprAuditTraceDto item)
        {
            validator.Validate(item);

            var entity = new GdprAudit
            {
                Id = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                DataHandlerId = currentUser.PersonId,
                EducationalInstitutionId = item.EducationalInstitutionId ?? currentUserProfile.EducationalInstitutionId,
                UserId = currentUser.Id == Guid.Empty ? null : currentUser.Id,
                UserProfileId = currentUserProfile.IsInitialized ? currentUserProfile.Id : null,
                UnitOfWorkId = db.UnitOfWorkId,
                Data = item.Data.Select(GdprAuditaDataMapper.Project())
                    .ToArray(),
                SupervisorId = currentUserProfile.SupervisorId
            };

            GdprAuditMapper.Map(item, entity);

            if (entity.DataHandlerId != null)
                entity.DataHandlerPrivatePersonalIdentifier = await GetPersonTechnicalPrivatePersonalIdentifierAsync(entity.DataHandlerId.Value);

            if (string.IsNullOrEmpty(entity.DataOwnerPrivatePersonalIdentifier) && entity.DataOwnerId != null)
                entity.DataOwnerPrivatePersonalIdentifier = await GetPersonTechnicalPrivatePersonalIdentifierAsync(entity.DataOwnerId.Value);

            db.GdprAudits.Add(entity);
        }
    }
}
