using Izm.Rumis.Api.Common;
using Izm.Rumis.Domain.Constants.Classifiers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Models
{
    public class ApplicationCreateRequest
    {
        public IEnumerable<Guid> ApplicationSocialStatuses { get; set; } = Array.Empty<Guid>();
        public string ApplicationStatusHistory { get; set; }
        public int EducationalInstitutionId { get; set; }
        public string Notes { get; set; }
        public Guid ResourceSubTypeId { get; set; }
        public int? ResourceTargetPersonClassGrade { get; set; }

        [MaxLength(10)]
        public string ResourceTargetPersonClassParallel { get; set; }

        [MaxLength(200)]
        public string ResourceTargetPersonEducationalProgram { get; set; }
        public Guid? ResourceTargetPersonEducationalStatusId { get; set; }
        public Guid? ResourceTargetPersonEducationalSubStatusId { get; set; }

        [MaxLength(50)]
        public string ResourceTargetPersonGroup { get; set; }
        public PersonData ResourceTargetPerson { get; set; }
        public Guid ResourceTargetPersonTypeId { get; set; }
        public Guid? ResourceTargetPersonWorkStatusId { get; set; }
        public bool? SocialStatus { get; set; }
        public bool? SocialStatusApproved { get; set; }
        public PersonData SubmitterPerson { get; set; }
        public Guid SubmitterTypeId { get; set; }

        public class PersonData
        {
            [MaxLength(100)]
            public string FirstName { get; set; }

            [MaxLength(100)]
            public string LastName { get; set; }

            [Required]
            [MaxLength(11)]
            public string PrivatePersonalIdentifier { get; set; }
            public IEnumerable<ContactData> ContactInformation { get; set; } = Array.Empty<ContactData>();

            public class ContactData
            {
                public Guid TypeId { get; set; }

                [Required]
                [MaxLength(200)]
                public string Value { get; set; }
            }
        }
    }

    public class ApplicationCreateResponse
    {
        public Guid Id { get; set; }
        public string ApplicationNumber { get; set; }
    }

    public class ApplicationFilterRequest : Filter<Domain.Entities.Application>
    {
        public DateTime? ApplicationDateFrom { get; set; }
        public DateTime? ApplicationDateTo { get; set; }
        public string ApplicationNumber { get; set; }
        public IEnumerable<Guid> ApplicationSocialStatusIds { get; set; }
        public IEnumerable<Guid> ApplicationSocialStatusApprovedIds { get; set; }
        public IEnumerable<Guid> ApplicationStatusIds { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public IEnumerable<int> EducationalInstitutionIds { get; set; }
        public bool? HasDuplicate { get; set; }
        public IEnumerable<int> SupervisorIds { get; set; }
        public IEnumerable<Guid> ResourceSubTypeIds { get; set; }
        public string ResourceTargetPerson { get; set; }
        public string ResourceTargetPersonClass { get; set; }
        public IEnumerable<Guid> ResourceTargetPersonTypeIds { get; set; }
        public string ResourceTargetPersonEducationalProgram { get; set; }
        public IEnumerable<Guid> ResourceTargetPersonEducationalStatusIds { get; set; }
        public IEnumerable<Guid> ResourceTargetPersonWorkStatusIds { get; set; }
        public string SubmitterPerson { get; set; }
        public IEnumerable<Guid> SubmitterTypeIds { get; set; }

        protected override Expression<Func<Domain.Entities.Application, bool>>[] GetFilters()
        {
            var filters = new List<Expression<Func<Domain.Entities.Application, bool>>>();

            if (ApplicationDateFrom != null)
                filters.Add(t => ApplicationDateFrom <= t.ApplicationDate);

            if (ApplicationDateTo != null)
                filters.Add(t => ApplicationDateTo > t.ApplicationDate.AddDays(-1));

            if (!string.IsNullOrEmpty(ApplicationNumber))
                filters.Add(t => t.ApplicationNumber.Contains(ApplicationNumber));

            if (ApplicationSocialStatusIds != null && ApplicationSocialStatusIds.Any())
                filters.Add(t => t.ApplicationSocialStatuses.Any(t => ApplicationSocialStatusIds.Contains(t.SocialStatusId)));

            if (ApplicationSocialStatusApprovedIds != null && ApplicationSocialStatusApprovedIds.Any())
                filters.Add(t => t.ApplicationSocialStatuses.Any(t => ApplicationSocialStatusApprovedIds.Contains(t.SocialStatusId)
                                                                        && (!t.SocialStatusApproved.HasValue || t.SocialStatusApproved.Value)));

            if (ApplicationStatusIds != null && ApplicationStatusIds.Any())
                filters.Add(t => ApplicationStatusIds.Contains(t.ApplicationStatusId));

            if (!string.IsNullOrEmpty(ContactEmail))
                filters.Add(t => t.ContactPerson.PersonContacts.Any(c => c.ContactType.Code == ContactType.Email
                                                                        && c.IsActive
                                                                        && c.ContactValue.Contains(ContactEmail)));

            if (!string.IsNullOrEmpty(ContactPhone))
                filters.Add(t => t.ContactPerson.PersonContacts.Any(c => c.ContactType.Code == ContactType.PhoneNumber
                                                                        && c.IsActive
                                                                        && c.ContactValue.Contains(ContactPhone)));

            if (EducationalInstitutionIds != null && EducationalInstitutionIds.Any())
                filters.Add(t => EducationalInstitutionIds.Contains(t.EducationalInstitutionId));

            if (HasDuplicate != null)
                filters.Add(t => t.ApplicationDuplicateId.HasValue == HasDuplicate);

            if (SupervisorIds != null && SupervisorIds.Any())
                filters.Add(t => SupervisorIds.Contains(t.EducationalInstitution.SupervisorId));

            if (ResourceSubTypeIds != null && ResourceSubTypeIds.Any())
                filters.Add(t => ResourceSubTypeIds.Contains(t.ResourceSubTypeId));

            if (!string.IsNullOrEmpty(ResourceTargetPerson))
                filters.Add(t => t.ResourceTargetPerson.Persons.Any(t => t.FirstName.Contains(ResourceTargetPerson)
                                                                      || t.LastName.Contains(ResourceTargetPerson)
                                                                      || t.PrivatePersonalIdentifier.Contains(ResourceTargetPerson)));

            if (!string.IsNullOrEmpty(ResourceTargetPersonClass))
                filters.Add(t => ResourceTargetPersonClass.Contains(
                                    t.ResourceTargetPersonClassGrade.HasValue
                                    ? t.ResourceTargetPersonClassGrade + " " + t.ResourceTargetPersonClassParallel
                                    : t.ResourceTargetPersonGroup));

            if (ResourceTargetPersonTypeIds != null && ResourceTargetPersonTypeIds.Any())
                filters.Add(t => ResourceTargetPersonTypeIds.Contains(t.ResourceTargetPersonTypeId));

            if (!string.IsNullOrEmpty(ResourceTargetPersonEducationalProgram))
                filters.Add(t => ResourceTargetPersonClass.Contains(t.ResourceTargetPersonEducationalProgram));

            if (ResourceTargetPersonEducationalStatusIds != null && ResourceTargetPersonEducationalStatusIds.Any())
                filters.Add(t => ResourceTargetPersonEducationalStatusIds.Contains(t.ResourceTargetPersonEducationalStatusId.Value));

            if (ResourceTargetPersonWorkStatusIds != null && ResourceTargetPersonWorkStatusIds.Any())
                filters.Add(t => ResourceTargetPersonWorkStatusIds.Contains(t.ResourceTargetPersonWorkStatusId.Value));

            if (!string.IsNullOrEmpty(SubmitterPerson))
                filters.Add(t => t.SubmitterPerson.Persons.Any(t => t.FirstName.Contains(SubmitterPerson)
                                                                 || t.LastName.Contains(SubmitterPerson)
                                                                 || t.PrivatePersonalIdentifier.Contains(SubmitterPerson)));

            if (SubmitterTypeIds != null && SubmitterTypeIds.Any())
                filters.Add(t => SubmitterTypeIds.Contains(t.SubmitterTypeId));

            return filters.ToArray();
        }
    }

    public abstract class ApplicationListItem
    {
        public DateTime ApplicationDate { get; set; }
        public string ApplicationNumber { get; set; }
        public IEnumerable<ApplicationSocialStatusData> ApplicationSocialStatus { get; set; }
        public ClassifierData ApplicationStatus { get; set; }
        public string ApplicationStatusHistory { get; set; }
        public ContactPersonTechnical ContactPerson { get; set; }
        public DateTime Created { get; set; }
        public Guid CreatedById { get; set; }
        public EducationalInstitutionData EducationalInstitution { get; set; }
        public bool HasDuplicate { get; set; }
        public Guid Id { get; set; }
        public DateTime Modified { get; set; }
        public Guid ModifiedById { get; set; }
        public int? MonitoringClassGrade { get; set; }
        public string MonitoringClassParallel { get; set; }
        public ClassifierData MonitoringEducationalStatus { get; set; }
        public ClassifierData MonitoringEducationalSubStatus { get; set; }
        public string MonitoringGroup { get; set; }
        public ClassifierData MonitoringWorkStatus { get; set; }
        public string Notes { get; set; }
        public PersonTechnical ResourceTargetPerson { get; set; }
        public int? ResourceTargetPersonClassGrade { get; set; }
        public string ResourceTargetPersonClassParallel { get; set; }
        public string ResourceTargetPersonEducationalProgram { get; set; }
        public ClassifierData ResourceTargetPersonEducationalStatus { get; set; }
        public ClassifierData ResourceTargetPersonEducationalSubStatus { get; set; }
        public string ResourceTargetPersonGroup { get; set; }
        public ClassifierData ResourceTargetPersonType { get; set; }
        public ClassifierData ResourceTargetPersonWorkStatus { get; set; }
        public bool? SocialStatus { get; set; }
        public bool? SocialStatusApproved { get; set; }
        public PersonTechnical SubmitterPerson { get; set; }
        public ClassifierData SubmitterType { get; set; }
        public SupervisorData Supervisor { get; set; }

        public class ApplicationSocialStatusData
        {
            public Guid Id { get; set; }
            public ClassifierData SocialStatus { get; set; }
        }

        public class ClassifierData
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Value { get; set; }
        }

        public class ClassifierDataWithPayload : ClassifierData
        {
            public string Payload { get; set; }
        }

        public class EducationalInstitutionData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class PersonTechnical
        {
            public Guid Id { get; set; }
            public IEnumerable<PersonData> Person { get; set; }

            public class PersonData
            {
                public Guid Id { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string PrivatePersonalIdentifier { get; set; }
            }
        }

        public class ContactPersonTechnical : PersonTechnical
        {
            public IEnumerable<ContactData> Contacts { get; set; }

            public class ContactData
            {
                public Guid Id { get; set; }
                public string ContactValue { get; set; }
                public ClassifierData ContactType { get; set; }
            }

        }

        public class SupervisorData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }

    public class ApplicationListItemResponse : ApplicationListItem
    {
        public ClassifierData ResourceType { get; set; }
        public ClassifierData ResourceSubType { get; set; }
    }

    public class ApplicationIntermediateListItemResponse : ApplicationListItem
    {
        public ClassifierDataWithPayload ResourceSubType { get; set; }
    }

    public class ApplicationResponse
    {
        public DateTime ApplicationDate { get; set; }
        public string ApplicationNumber { get; set; }
        public IEnumerable<ApplicationResourceData> ApplicationResources { get; set; }
        public IEnumerable<ApplicationSocialStatusData> ApplicationSocialStatus { get; set; }
        public ClassifierData ApplicationStatus { get; set; }
        public string ApplicationStatusHistory { get; set; }
        public PersonTechnicalWithContactData ContactPerson { get; set; }
        public DateTime Created { get; set; }
        public Guid CreatedById { get; set; }
        public EducationalInstitutionData EducationalInstitution { get; set; }
        public DateTime Modified { get; set; }
        public Guid ModifiedById { get; set; }
        public int? MonitoringClassGrade { get; set; }
        public string MonitoringClassParallel { get; set; }
        public ClassifierData MonitoringEducationalStatus { get; set; }
        public ClassifierData MonitoringEducationalSubStatus { get; set; }
        public string MonitoringGroup { get; set; }
        public ClassifierData MonitoringWorkStatus { get; set; }
        public string Notes { get; set; }
        public ClassifierData ResourceSubType { get; set; }
        public PersonTechnical ResourceTargetPerson { get; set; }
        public int? ResourceTargetPersonClassGrade { get; set; }
        public string ResourceTargetPersonClassParallel { get; set; }
        public string ResourceTargetPersonEducationalProgram { get; set; }
        public ClassifierData ResourceTargetPersonEducationalStatus { get; set; }
        public ClassifierData ResourceTargetPersonEducationalSubStatus { get; set; }
        public string ResourceTargetPersonGroup { get; set; }
        public ClassifierData ResourceTargetPersonType { get; set; }
        public ClassifierData ResourceTargetPersonWorkStatus { get; set; }
        public bool? SocialStatus { get; set; }
        public bool? SocialStatusApproved { get; set; }
        public PersonTechnical SubmitterPerson { get; set; }
        public ClassifierData SubmitterType { get; set; }

        public class ApplicationResourceData
        {
            public Guid Id { get; set; }
            public string PNANumber { get; set; }
            public ClassifierData PNAStatus { get; set; }
        }

        public class ApplicationSocialStatusData
        {
            public Guid Id { get; set; }
            public ClassifierData SocialStatus { get; set; }
            public bool? SocialStatusApproved { get; set; }
        }

        public class ClassifierData
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Value { get; set; }
        }

        public class EducationalInstitutionData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class PersonTechnicalWithContactData
        {
            public Guid Id { get; set; }
            public IEnumerable<PersonData> Person { get; set; }
            public IEnumerable<ContactInformation> ContactData { get; set; }

            public class PersonData
            {
                public Guid Id { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string PrivatePersonalIdentifier { get; set; }
            }

            public class ContactInformation
            {
                public ClassifierData Type { get; set; }
                public string Value { get; set; }
            }
        }

        public class PersonTechnical
        {
            public Guid Id { get; set; }
            public IEnumerable<PersonData> Person { get; set; }

            public class PersonData
            {
                public Guid Id { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string PrivatePersonalIdentifier { get; set; }
            }
        }
    }

    public class ApplicationSocialStatusResponse
    {
        public Guid Id { get; set; }
        public bool? SocialStatusApproved { get; set; }
        public IEnumerable<ApplicationSocialStatusData> ApplicationSocialStatus { get; set; }

        public class ApplicationSocialStatusData
        {
            public Guid Id { get; set; }
            public ClassifierData SocialStatus { get; set; }
            public bool? SocialStatusApproved { get; set; }

            public class ClassifierData
            {
                public Guid Id { get; set; }
                public string Code { get; set; }
                public string Value { get; set; }
            }
        }
    }

    public class ApplicationUpdateRequest
    {
        public string ApplicationStatusHistory { get; set; }
        public Guid ApplicationStatusId { get; set; }
        public Guid ContactPersonId { get; set; }
        public string Notes { get; set; }
        public Guid ResourceSubTypeId { get; set; }
        public int? ResourceTargetPersonClassGrade { get; set; }

        [MaxLength(10)]
        public string ResourceTargetPersonClassParallel { get; set; }

        [MaxLength(200)]
        public string ResourceTargetPersonEducationalProgram { get; set; }
        public Guid? ResourceTargetPersonEducationalStatusId { get; set; }
        public Guid? ResourceTargetPersonEducationalSubStatusId { get; set; }

        [MaxLength(50)]
        public string ResourceTargetPersonGroup { get; set; }
        public Guid ResourceTargetPersonId { get; set; }
        public Guid ResourceTargetPersonTypeId { get; set; }
        public Guid? ResourceTargetPersonWorkStatusId { get; set; }
        public bool? SocialStatus { get; set; }
        public bool? SocialStatusApproved { get; set; }
        public Guid SubmitterTypeId { get; set; }
    }

    public class ApplicationDeclineRequest
    {
        public IEnumerable<Guid> ApplicationIds { get; set; }
        public string Reason { get; set; }
    }

    public class ApplicationContactPersonUpdateRequest
    {
        [Required]
        [MaxLength(11)]
        public string PrivatePersonalIdentifier { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }
        public IEnumerable<ContactInformation> ApplicationContactInformation { get; set; } = Array.Empty<ContactInformation>();

        public class ContactInformation
        {
            public Guid TypeId { get; set; }
            public string Value { get; set; }
        }
    }

    public class ApplicationsContactPersonUpdateRequest
    {
        public Guid ResourceTargetPersonId { get; set; }
        public ContactPersonData ContactPerson { get; set; }

        public class ContactPersonData
        {
            [Required]
            [MaxLength(11)]
            public string PrivatePersonalIdentifier { get; set; }

            [MaxLength(100)]
            public string FirstName { get; set; }

            [MaxLength(100)]
            public string LastName { get; set; }
            public IEnumerable<ContactInformation> ApplicationContactInformation { get; set; } = Array.Empty<ContactInformation>();

            public class ContactInformation
            {
                public Guid TypeId { get; set; }
                public string Value { get; set; }
            }
        }
    }

    public class ApplicationCheckDuplicateRequest
    {
        public string PrivatePersonalIdentifier { get; set; }
        public Guid ResourceSubTypeId { get; set; }

    }

    public class ApplicationCheckDuplicateResponse
    {
        public EducationalInstitutionData EducationalInstitution { get; set; }

        public class EducationalInstitutionData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }
}
