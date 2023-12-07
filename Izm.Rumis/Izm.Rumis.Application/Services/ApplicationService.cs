using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Helpers;
using Izm.Rumis.Application.Mappers;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Application.Models.Application;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
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
    public class ApplicationService : IApplicationService
    {
        private readonly IAppDbContext db;
        private readonly ISequenceService sequenceService;
        private readonly IApplicationValidator validator;
        private readonly ICurrentUserProfileService currentUserProfile;
        private readonly IAuthorizationService authorizationService;
        private readonly IApplicationSocialStatusCheckService applicationSocialStatusCheckService;
        private readonly IPersonDataService personDataService;
        private readonly IGdprAuditService gdprAuditService;
        private readonly IApplicationDuplicateService applicationDuplicateService;

        public ApplicationService(
            IAppDbContext db,
            IApplicationValidator validator,
            ICurrentUserProfileService currentUserProfile,
            IAuthorizationService authorizationService,
            IApplicationSocialStatusCheckService applicationSocialStatusCheckService,
            IPersonDataService personDataService,
            IGdprAuditService gdprAuditService,
            ISequenceService sequenceService,
            IApplicationDuplicateService applicationDuplicateService)
        {
            this.db = db;
            this.validator = validator;
            this.currentUserProfile = currentUserProfile;
            this.authorizationService = authorizationService;
            this.applicationSocialStatusCheckService = applicationSocialStatusCheckService;
            this.personDataService = personDataService;
            this.gdprAuditService = gdprAuditService;
            this.sequenceService = sequenceService;
            this.applicationDuplicateService = applicationDuplicateService;
        }

        /// <inheritdoc/>
        public async Task ChangeSubmitterContactAsync(Guid id, ApplicationContactInformationUpdateDto item, CancellationToken cancellationToken = default)
        {
            var entity = await db.Applications.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            authorizationService.Authorize(entity.EducationalInstitutionId);

            Dictionary<string, PersonTechnical> personTechnicalMap =
                await GeneratePersonTechnicalMapAsync(cancellationToken, item.Person);

            entity.ContactPerson = personTechnicalMap[item.Person.PrivatePersonalIdentifier];

            // TODO?: This will not trace updated old entries - Persons, PersonContacts.
            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForChangeSubmitterContactOperation(id, entity.ContactPerson.Id, entity.EducationalInstitutionId, item.Person));

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task ChangeSubmittersContactAsync(IEnumerable<Guid> ids, ApplicationsContactInformationUpdateDto item, CancellationToken cancellationToken = default)
        {
            var applications = await db.Applications
                .Where(t => ids.Contains(t.Id))
                .ToArrayAsync(cancellationToken);

            Dictionary<string, PersonTechnical> personTechnicalMap =
                await GeneratePersonTechnicalMapAsync(cancellationToken, item.Person);

            foreach (var application in applications)
            {
                authorizationService.Authorize(application.EducationalInstitutionId);

                application.ContactPerson = personTechnicalMap[item.Person.PrivatePersonalIdentifier];

                await gdprAuditService.TraceAsync(
                    GdprAuditHelper.GenerateTraceForChangeSubmitterContactOperation(
                        applicationId: application.Id,
                        dataOwnerId: application.ContactPerson.Id,
                        educationalInstitutionId: application.EducationalInstitutionId,
                        personData: item.Person),
                    cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task<Domain.Entities.Application> CheckApplicationSocialStatusAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.Applications.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            var currentStatuses = entity.ApplicationSocialStatuses.Select(t => t.SocialStatus.Code).Distinct().ToArray();

            var currentSocialStatusTypes = SocialStatus.ViisStatusPairs.Join(currentStatuses, viis => viis.Key,
                    curStatus => curStatus, (viis, curStatus) => (viis.Value)).ToArray();

            var resourceTargetPerson = entity.ResourceTargetPerson.GetPerson();

            var personPrivateIntetifier = resourceTargetPerson.PrivatePersonalIdentifier;

            var socialStatusesResponse = applicationSocialStatusCheckService.CheckSocialStatusesAsync(personPrivateIntetifier, currentSocialStatusTypes, cancellationToken).Result;

            var socialStatuses = SocialStatus.ViisStatusPairs.Join(socialStatusesResponse, viis => viis.Value,
                   curStatus => curStatus.Key, (viis, curStatus) => (viis.Key, curStatus.Value)).ToDictionary(t => t.Key);

            foreach (var applicationSocialStatus in entity.ApplicationSocialStatuses)
            {
                applicationSocialStatus.SocialStatusApproved = socialStatuses.ContainsKey(applicationSocialStatus.SocialStatus.Code) ? socialStatuses[applicationSocialStatus.SocialStatus.Code].Value : null;
            }

            entity.SocialStatusApproved = entity.ApplicationSocialStatuses.Any(t => t.SocialStatusApproved == false) ? false :
                entity.ApplicationSocialStatuses.Any(t => t.SocialStatusApproved == true) ? true : null;

            await db.SaveChangesAsync(cancellationToken);

            return entity;
        }

        /// <inheritdoc/>
        public async Task<ApplicationCreateResult> CreateAsync(ApplicationCreateDto item, CancellationToken cancellationToken = default)
        {
            await validator.ValidateAsync(item, cancellationToken);

            authorizationService.Authorize(item.EducationalInstitutionId);

            var submitterTypeCode = await db.Classifiers
                .Where(x => x.Id == item.SubmitterTypeId)
                .Select(x => x.Code)
                .FirstAsync();

            var educationalInstitutionCode = await db.EducationalInstitutions
                .Where(t => t.Id == item.EducationalInstitutionId)
                .Select(t => t.Code)
                .FirstAsync();

            if (submitterTypeCode == ApplicantRole.ParentGuardian)
                await authorizationService.AuthorizeAsync(
                        item.SubmitterPerson.PrivatePersonalIdentifier,
                        item.ResourceTargetPerson.PrivatePersonalIdentifier,
                        cancellationToken);

            if (submitterTypeCode == ApplicantRole.Employee)
                await authorizationService.AuthorizeAsync(
                        ApplicantRole.Employee,
                        item.ResourceTargetPerson.PrivatePersonalIdentifier,
                        educationalInstitutionCode,
                        cancellationToken);
            else
                await authorizationService.AuthorizeAsync(
                        ApplicantRole.Learner,
                        item.ResourceTargetPerson.PrivatePersonalIdentifier,
                        educationalInstitutionCode,
                        cancellationToken);

            var serialNumberWithinInsitution = sequenceService.GetByKey(NumberingPatternHelper.ApplicationKeyFormat(educationalInstitutionCode));

            var entity = new Domain.Entities.Application
            {
                ApplicationNumber = NumberingPatternHelper.ApplicationNumberFormat(educationalInstitutionCode, serialNumberWithinInsitution),
                ApplicationDate = DateTime.UtcNow,
            };

            ApplicationMapper.Map(item, entity);

            var statusId = await db.Classifiers
                .Where(t => t.Type == ClassifierTypes.ApplicationStatus
                    && t.Code == ApplicationStatus.Submitted)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(cancellationToken);

            entity.SetApplicationStatus(statusId);

            Dictionary<string, PersonTechnical> personTechnicalMap =
                await GeneratePersonTechnicalMapAsync(cancellationToken, item.ResourceTargetPerson, item.SubmitterPerson);

            var resourceTargetPersonTypeCode = await db.Classifiers
                .Where(x => x.Id == item.ResourceTargetPersonTypeId)
                .Select(x => x.Code)
                .FirstAsync();

            entity.ResourceTargetPerson = personTechnicalMap[item.ResourceTargetPerson.PrivatePersonalIdentifier];
            entity.SubmitterPerson = personTechnicalMap[item.SubmitterPerson.PrivatePersonalIdentifier];
            entity.ContactPerson = entity.SubmitterPerson;

            if (resourceTargetPersonTypeCode == ResourceTargetPersonType.Learner)
            {
                var birthDate = await personDataService.GetBirthDateAsync(item.SubmitterPerson.PrivatePersonalIdentifier, item.ResourceTargetPerson.PrivatePersonalIdentifier, submitterTypeCode, cancellationToken);

                foreach (var person in entity.ResourceTargetPerson.Persons)
                    person.BirthDate = birthDate;
            }

            entity.ApplicationSocialStatuses = item.ApplicationSocialStatuses.Select(t => new ApplicationSocialStatus
            {
                ApplicationId = entity.Id,
                SocialStatusId = t
            }).ToArray();

            db.Applications.Add(entity);

            await gdprAuditService.TraceRangeAsync(
                personTechnicalMap.Values.Select(GdprAuditHelper.ProjectCreateTraces(entity.EducationalInstitutionId)).ToArray(),
                cancellationToken
                );

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(new[] { entity.Id }, cancellationToken);

            return new ApplicationCreateResult(entity.Id, entity.ApplicationNumber);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task DeclineAsync(ApplicationDeclineDto item, CancellationToken cancellationToken = default)
        {
            var applications = await db.Applications.Where(t => item.ApplicationIds.Contains(t.Id)).ToArrayAsync(cancellationToken);

            if (applications.Count() != item.ApplicationIds.Count())
                throw new EntityNotFoundException();

            var statusId = await db.Classifiers
                .Where(t => t.Type == ClassifierTypes.ApplicationStatus
                    && t.Code == ApplicationStatus.Declined)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(cancellationToken);

            foreach (var entity in applications)
            {
                validator.Validate(entity.ApplicationStatus.Code, ApplicationStatus.Declined);

                entity.SetApplicationStatus(statusId);

                entity.DeclineReason = item.Reason;
            }

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(applications.Select(t => t.Id), cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task DeleteAsync(IEnumerable<Guid> applicationIds, CancellationToken cancellationToken = default)
        {
            var applications = await db.Applications.Where(t => applicationIds.Contains(t.Id)).ToArrayAsync(cancellationToken);

            if (applications.Count() != applicationIds.Count())
                throw new EntityNotFoundException();

            var statusId = await db.Classifiers
                .Where(t => t.Type == ClassifierTypes.ApplicationStatus
                    && t.Code == ApplicationStatus.Deleted)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(cancellationToken);

            foreach (var entity in applications)
            {
                validator.Validate(entity.ApplicationStatus.Code, ApplicationStatus.Deleted);

                entity.SetApplicationStatus(statusId);
            }

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(applications.Select(t => t.Id), cancellationToken);
        }

        /// <inheritdoc/>
        public SetQuery<Domain.Entities.Application> Get()
        {
            var query = db.Applications.AsNoTracking();

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

            return new SetQuery<Domain.Entities.Application>(query);
        }

        /// <inheritdoc/>
        public SetQuery<Domain.Entities.Application> GetApplicationDuplicates(ApplicationCheckDuplicateDto item)
        {
            var query = db.Applications
                .AsNoTracking()
                .Where(t => t.ResourceSubTypeId == item.ResourceSubTypeId
                    && t.ResourceTargetPerson.Persons.Any(p => p.PrivatePersonalIdentifier == item.PrivatePersonalIdentifier)
                    && (ApplicationStatus.ActiveStatuses.Contains(t.ApplicationStatus.Code)
                            || t.ApplicationResources.Any(ar => PnaStatus.ActiveStatuses.Contains(ar.PNAStatus.Code))));

            return new SetQuery<Domain.Entities.Application>(query);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task PostponeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.Applications.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            validator.Validate(entity.ApplicationStatus.Code, ApplicationStatus.Postponed);

            var statusId = await db.Classifiers
                .Where(t => t.Type == ClassifierTypes.ApplicationStatus
                    && t.Code == ApplicationStatus.Postponed)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(cancellationToken);

            entity.SetApplicationStatus(statusId);

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(new[] { entity.Id }, cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task UpdateAsync(Guid id, ApplicationUpdateDto item, CancellationToken cancellationToken = default)
        {
            var entity = await db.Applications.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            ApplicationMapper.Map(item, entity);

            entity.SetApplicationStatus(item.ApplicationStatusId);

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(new[] { entity.Id }, cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task WithdrawAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await db.Applications.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            validator.Validate(entity.ApplicationStatus.Code, ApplicationStatus.Withdrawn);

            var statusId = await db.Classifiers
                .Where(t => t.Type == ClassifierTypes.ApplicationStatus
                    && t.Code == ApplicationStatus.Withdrawn)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(cancellationToken);

            entity.SetApplicationStatus(statusId);

            await db.SaveChangesAsync(cancellationToken);

            await applicationDuplicateService.CheckApplicationsDuplicatesAsync(new[] { entity.Id }, cancellationToken);
        }

        private IEnumerable<PersonData> GetDistinctPersons(params PersonData[] personDatas)
        {
            return personDatas
                .GroupBy(t => t.PrivatePersonalIdentifier)
                .Select(t =>
                {
                    var first = t.First();

                    first.ContactInformation = t.SelectMany(n => n.ContactInformation)
                        .ToArray();

                    return first;
                })
                .ToArray();
        }

        private async Task<Dictionary<string, PersonTechnical>> GeneratePersonTechnicalMapAsync(
            CancellationToken cancellationToken = default,
            params PersonData[] personData
            )
        {
            var distinctPersonData = GetDistinctPersons(personData);

            var privatePersonalIdentifiers = distinctPersonData
                .Select(t => t.PrivatePersonalIdentifier)
                .ToArray();

            var personTechnicals = await db.PersonTechnicals
                .Where(personTechnical => personTechnical.Persons.Any(
                    person => privatePersonalIdentifiers.Contains(person.PrivatePersonalIdentifier)
                    ))
                .Include(t => t.Persons)
                .Include(t => t.PersonContacts)
                .ToArrayAsync(cancellationToken);

            var personTechnicalMap = new Dictionary<string, PersonTechnical>();

            foreach (var person in distinctPersonData)
            {
                var personTechnical = personTechnicals.FirstOrDefault(
                    t => t.Persons.Any(n => n.PrivatePersonalIdentifier == person.PrivatePersonalIdentifier)
                    );

                if (personTechnical == null)
                {
                    personTechnical = new PersonTechnical();

                    db.Persons.Add(new Person
                    {
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        PrivatePersonalIdentifier = person.PrivatePersonalIdentifier,
                        PersonTechnical = personTechnical
                    });
                }

                var contactsToAdd = person.ContactInformation
                    .Where(t => !personTechnical.PersonContacts.Any(n => n.ContactTypeId == t.TypeId && n.ContactValue == t.Value))
                    .ToArray();

                foreach (var contact in contactsToAdd)
                    personTechnical.PersonContacts.Add(new PersonContact
                    {
                        ContactTypeId = contact.TypeId,
                        ContactValue = contact.Value,
                        IsActive = true
                    });

                var contactsToSetInactive = personTechnical.PersonContacts
                    .Where(t => t.IsActive && person.ContactInformation.Any(x => x.TypeId == t.ContactTypeId && x.Value != t.ContactValue))
                    .ToArray();

                foreach (var contact in contactsToSetInactive)
                    contact.IsActive = false;

                var contactsToSetActive = personTechnical.PersonContacts
                    .Where(t => !t.IsActive && person.ContactInformation.Any(x => x.TypeId == t.ContactTypeId && x.Value == t.ContactValue))
                    .ToArray();

                foreach (var contact in contactsToSetActive)
                    contact.IsActive = true;

                personTechnicalMap.Add(person.PrivatePersonalIdentifier, personTechnical);
            }

            return personTechnicalMap;
        }

        public static class GdprAuditAction
        {
            public const string Create = "application.create";
        }

        public static class GdprAuditHelper
        {
            public static GdprAuditTraceDto GenerateTraceForChangeSubmitterContactOperation(Guid applicationId, Guid dataOwnerId, int educationalInstitutionId, PersonData personData)
            {
                var result = new GdprAuditTraceDto
                {
                    Action = "application.changeSubmitterContact",
                    ActionData = JsonSerializer.Serialize(new { ApplicationId = applicationId }),
                    DataOwnerId = dataOwnerId,
                    DataOwnerPrivatePersonalIdentifier = null,
                    EducationalInstitutionId = educationalInstitutionId
                };

                var data = new List<PersonDataProperty>
                {
                    new PersonDataProperty { Type = PersonDataType.FirstName, Value = personData.FirstName },
                    new PersonDataProperty { Type = PersonDataType.LastName, Value = personData.LastName },
                    new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = personData.PrivatePersonalIdentifier }
                };

                foreach (var contact in personData.ContactInformation)
                    data.Add(new PersonDataProperty { Type = PersonDataType.Contact, Value = contact.Value });

                result.Data = data.Where(t => t.Value != null)
                    .ToArray();

                return result;
            }

            public static Func<PersonTechnical, GdprAuditTraceDto> ProjectCreateTraces(int educationalInstitutionId)
            {
                return personTechnical =>
                {
                    var result = new GdprAuditTraceDto
                    {
                        Action = "application.create",
                        ActionData = null,
                        DataOwnerId = personTechnical.Id,
                        EducationalInstitutionId = educationalInstitutionId
                    };

                    var data = new List<PersonDataProperty>();

                    foreach (var person in personTechnical.Persons)
                    {
                        data.Add(new PersonDataProperty { Type = PersonDataType.BirthDate, Value = person.BirthDate.ToString() });
                        data.Add(new PersonDataProperty { Type = PersonDataType.FirstName, Value = person.FirstName });
                        data.Add(new PersonDataProperty { Type = PersonDataType.LastName, Value = person.LastName });
                        data.Add(new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = person.PrivatePersonalIdentifier });
                    }

                    foreach (var contact in personTechnical.PersonContacts)
                        data.Add(new PersonDataProperty { Type = PersonDataType.Contact, Value = contact.ContactValue });

                    result.Data = data.Where(t => t.Value != null)
                        .Distinct()
                        .ToArray();

                    return result;
                };
            }
        }
    }
}
