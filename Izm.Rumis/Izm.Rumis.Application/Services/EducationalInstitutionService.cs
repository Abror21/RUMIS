using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Mappers;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public sealed class EducationalInstitutionService : IEducationalInstitutionService
    {
        private readonly IAppDbContext db;
        private readonly ICurrentUserProfileService currentUserProfile;
        private readonly IGdprAuditService gdprAuditService;

        public EducationalInstitutionService(IAppDbContext db, ICurrentUserProfileService currentUserProfile, IGdprAuditService gdprAuditService)
        {
            this.db = db;
            this.currentUserProfile = currentUserProfile;
            this.gdprAuditService = gdprAuditService;
        }


        /// <inheritdoc/>
        public async Task<int> CreateAsync(EducationalInstitutionCreateDto item, CancellationToken cancellationToken = default)
        {
            var entity = new EducationalInstitution();

            var status = await db.Classifiers
                .FirstOrDefaultAsync(t => t.Id == item.StatusId
                    || (t.Type == ClassifierTypes.EducationalInstitutionStatus && t.Code == EducationalInstitutionStatus.Disabled), cancellationToken);

            if (status == null)
                throw new EntityNotFoundException();

            entity.Status = status;

            await db.EducationalInstitutions.AddAsync(EducationalInstitutionMapper.Map(item, entity), cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await db.EducationalInstitutions.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            db.EducationalInstitutions.Remove(entity);

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public SetQuery<EducationalInstitution> Get()
        {
            var query = db.EducationalInstitutions.AsNoTracking();

            switch (currentUserProfile.Type)
            {
                case UserProfileType.Supervisor:
                    query = query.Where(t => t.SupervisorId == currentUserProfile.SupervisorId);
                    break;
                case UserProfileType.EducationalInstitution:
                    query = query.Where(t => t.Id == currentUserProfile.EducationalInstitutionId);
                    break;
                case UserProfileType.Country:
                default:
                    break;
            }

            return new SetQuery<EducationalInstitution>(query);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(int id, EducationalInstitutionUpdateDto item, CancellationToken cancellationToken = default)
        {
            var entity = await db.EducationalInstitutions
                .Include(t => t.EducationalInstitutionContactPersons)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            EducationalInstitutionMapper.Map(item, entity);

            var educationalInstitutionContactPersonsToDelete = entity.EducationalInstitutionContactPersons
                .Where(t => !item.EducationalInstitutionContactPersons.Any(n => n.Id == t.Id));

            foreach (var educationalInstitutionContactPerson in educationalInstitutionContactPersonsToDelete)
            {
                db.ContactPersonResourceSubTypes.RemoveRange(educationalInstitutionContactPerson.ContactPersonResourceSubTypes);

                db.EducationalInstitutionContactPersons.Remove(educationalInstitutionContactPerson);
            }

            var educationalInstitutionContactPersonsToAdd = item.EducationalInstitutionContactPersons
                .Where(t => !entity.EducationalInstitutionContactPersons.Any(n => n.Id == t.Id));

            foreach (var educationalInstitutionContactPerson in educationalInstitutionContactPersonsToAdd)
                entity.EducationalInstitutionContactPersons.Add(new EducationalInstitutionContactPerson
                {
                    Name = educationalInstitutionContactPerson.Name,
                    Email = educationalInstitutionContactPerson.Email,
                    PhoneNumber = educationalInstitutionContactPerson.PhoneNumber,
                    Address = educationalInstitutionContactPerson.Address,
                    JobPositionId = educationalInstitutionContactPerson.JobPositionId,
                    EducationalInstitutionId = id,
                    ContactPersonResourceSubTypes = educationalInstitutionContactPerson.ContactPersonResourceSubTypes.Select(t => new ContactPersonResourceSubType
                    {
                        ResourceSubTypeId = t.ResourceSubTypeId
                    }).ToArray()
                });

            var educationalInstitutionContactPersonsToUpdate = entity.EducationalInstitutionContactPersons
                .Where(t => item.EducationalInstitutionContactPersons.Any(n => n.Id == t.Id));

            foreach (var educationalInstitutionContactPerson in educationalInstitutionContactPersonsToUpdate)
            {
                var newEducationalInstitutionContactPersonData = item.EducationalInstitutionContactPersons
                    .First(t => educationalInstitutionContactPerson.Id == t.Id);

                educationalInstitutionContactPerson.Name = newEducationalInstitutionContactPersonData.Name;
                educationalInstitutionContactPerson.Email = newEducationalInstitutionContactPersonData.Email;
                educationalInstitutionContactPerson.PhoneNumber = newEducationalInstitutionContactPersonData.PhoneNumber;
                educationalInstitutionContactPerson.Address = newEducationalInstitutionContactPersonData.Address;
                educationalInstitutionContactPerson.JobPositionId = newEducationalInstitutionContactPersonData.JobPositionId;

                db.ContactPersonResourceSubTypes.RemoveRange(educationalInstitutionContactPerson.ContactPersonResourceSubTypes
                    .Where(t => !newEducationalInstitutionContactPersonData.ContactPersonResourceSubTypes.Any(n => n.Id == t.Id)));

                var contactPersonResourceSubTypesToAdd = newEducationalInstitutionContactPersonData.ContactPersonResourceSubTypes
                    .Where(t => !educationalInstitutionContactPerson.ContactPersonResourceSubTypes.Any(n => n.Id == t.Id));

                foreach (var contactPersonResourceSubType in contactPersonResourceSubTypesToAdd)
                    educationalInstitutionContactPerson.ContactPersonResourceSubTypes.Add(new ContactPersonResourceSubType
                    {
                        ResourceSubTypeId = contactPersonResourceSubType.ResourceSubTypeId,
                        EducationalInstitutionContactPersonId = educationalInstitutionContactPerson.Id
                    });

                var contactPersonResourceSubTypesToUpdate = educationalInstitutionContactPerson.ContactPersonResourceSubTypes
                    .Where(t => newEducationalInstitutionContactPersonData.ContactPersonResourceSubTypes.Any(n => n.Id == t.Id));

                foreach (var contactPersonResourceSubType in contactPersonResourceSubTypesToUpdate)
                {
                    var newContactPersonResourceSubTypeData = newEducationalInstitutionContactPersonData.ContactPersonResourceSubTypes
                    .First(t => contactPersonResourceSubType.Id == t.Id);

                    contactPersonResourceSubType.ResourceSubTypeId = newContactPersonResourceSubTypeData.ResourceSubTypeId;
                }
            }

            db.EducationalInstitutionResourceSubTypes.RemoveRange(entity.EducationalInstitutionResourceSubTypes
                    .Where(t => !item.EducationalInstitutionResourceSubTypes.Any(n => n.Id == t.Id)
                             || item.EducationalInstitutionResourceSubTypes.Any(n => n.Id == t.Id && !n.IsActive)));

            var educationalInstitutionResourceSubTypeToAdd = item.EducationalInstitutionResourceSubTypes
                .Where(t => t.IsActive && !entity.EducationalInstitutionResourceSubTypes.Any(n => n.Id == t.Id || n.ResourceSubTypeId == t.ResourceSubTypeId));

            foreach (var educationalInstitutionResourceSubType in educationalInstitutionResourceSubTypeToAdd)
                entity.EducationalInstitutionResourceSubTypes.Add(new EducationalInstitutionResourceSubType
                {
                    ResourceSubTypeId = educationalInstitutionResourceSubType.ResourceSubTypeId,
                    TargetPersonGroupTypeId = educationalInstitutionResourceSubType.TargetPersonGroupTypeId
                });

            var educationalInstitutionResourceSubTypeToUpdate = entity.EducationalInstitutionResourceSubTypes
                .Where(t => item.EducationalInstitutionResourceSubTypes.Any(n => n.Id == t.Id && n.IsActive));

            foreach (var educationalInstitutionResourceSubType in educationalInstitutionResourceSubTypeToUpdate)
            {
                var newEducationalInstitutionResourceSubTypeData = item.EducationalInstitutionResourceSubTypes
                    .First(t => educationalInstitutionResourceSubType.Id == t.Id);

                educationalInstitutionResourceSubType.ResourceSubTypeId = newEducationalInstitutionResourceSubTypeData.ResourceSubTypeId;
                educationalInstitutionResourceSubType.TargetPersonGroupTypeId = newEducationalInstitutionResourceSubTypeData.TargetPersonGroupTypeId;
            }

            await gdprAuditService.TraceRangeAsync(entity.EducationalInstitutionContactPersons.Select(GdprAuditHelper.ProjectTraces(id)).ToArray(), cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
        }

        public static class GdprAuditHelper
        {
            public static Func<EducationalInstitutionContactPerson, GdprAuditTraceDto> ProjectTraces(int educationalInstitutionId)
            {
                return contactPerson => new GdprAuditTraceDto
                {
                    Action = "educationalInstitution.update",
                    ActionData = JsonSerializer.Serialize(new { EducationalInstitutionId = educationalInstitutionId }),
                    DataOwnerId = null,
                    EducationalInstitutionId = educationalInstitutionId,
                    DataOwnerPrivatePersonalIdentifier = null,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.Name, Value = contactPerson.Name },
                        new PersonDataProperty { Type = PersonDataType.Contact, Value = contactPerson.Email },
                        new PersonDataProperty { Type = PersonDataType.Contact, Value = contactPerson.PhoneNumber },
                    }.Where(t => t.Value != null).ToArray()
                };
            }
        }
    }
}
