using Izm.Rumis.Domain.Constants.Classifiers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Izm.Rumis.Api.Models
{
    public class EServiceRelatedPersonResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PrivatePersonalIdentifier { get; set; }
        public string BirthDate { get; set; }
        public IEnumerable<ActiveEducationDataResponse> ActiveEducationData { get; set; }

        public class ActiveEducationDataResponse
        {
            public string ClassGroup { get; set; }
            public string ClassGroupLevel { get; set; }
            public string EducationProgram { get; set; }
            public string EducationProgramCode { get; set; }
            public string EducationInstitutionName { get; set; }
            public int? EducationalInstitutionId { get; set; }
            public string EducationInstitutionCode { get; set; }
            public ClassifierData EducationInstitutionStatus { get; set; }
            public string SupervisorName { get; set; }
            public string SupervisorCode { get; set; }

            public class ClassifierData
            {
                public Guid Id { get; set; }
                public string Code { get; set; }
                public string Value { get; set; }
            }
        }
    }

    public class EServiceEmployeeInstitutionDataResponse
    {
        public int? EducationalInstitutionId { get; set; }
        public string EducationInstitutionName { get; set; }
        public string EducationInstitutionCode { get; set; }
        public string SupervisorName { get; set; }
        public string SupervisorCode { get; set; }
    }


    public class EServiceEmployeeResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PrivatePersonalIdentifier { get; set; }
        public IEnumerable<ActiveWorkDataResponse> ActiveWorkData { get; set; }

        public class ActiveWorkDataResponse
        {
            public int EducationalInstitutionId { get; set; }
            public string EducationInstitutionCode { get; set; }
            public string EducationInstitutionName { get; set; }
            public string SupervisorName { get; set; }
            public string SupervisorCode { get; set; }
            public string PositionName { get; set; }
            public string PositionCode { get; set; }
        }
    }

    public class EServiceApplicationCreateRequest
    {
        public Guid ApplicationStatusId { get; set; }
        public string ApplicationStatusHistory { get; set; }
        public int EducationalInstitutionId { get; set; }
        public string Notes { get; set; }
        public Guid ResourceSubTypeId { get; set; }

        [MaxLength(10)]
        public string ResourceTargetPersonClassParallel { get; set; }

        [MaxLength(50)]
        public string ResourceTargetPersonGroup { get; set; }

        [MaxLength(200)]
        public string ResourceTargetPersonEducationalProgram { get; set; }

        public Guid? ResourceTargetPersonEducationalStatusId { get; set; }
        public Guid? ResourceTargetPersonEducationalSubStatusId { get; set; }
        public PersonData ResourceTargetPerson { get; set; }
        public Guid ResourceTargetPersonTypeId { get; set; }
        public Guid? ResourceTargetPersonWorkStatusId { get; set; }
        public int? ResourceTargetPersonClassGrade { get; set; }

        public Guid SubmitterTypeId { get; set; }
        public IEnumerable<PersonData.ContactData> SubmitterContactInformation { get; set; } = Array.Empty<PersonData.ContactData>();

        public bool? SocialStatus { get; set; }
        public bool? SocialStatusApproved { get; set; }
        public IEnumerable<Guid> ApplicationSocialStatuses { get; set; } = Array.Empty<Guid>();

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

    public class EServiceApplicationCreateResponse
    {
        public Guid Id { get; set; }
        public string ApplicationNumber { get; set; }
    }

    public abstract class EServiceApplication
    {
        public DateTime ApplicationDate { get; set; }
        public string ApplicationNumber { get; set; }
        public IEnumerable<ApplicationSocialStatusData> ApplicationSocialStatuses { get; set; }
        public ClassifierData ApplicationStatus { get; set; }
        public PersonTechnicalWithContactData ContactPerson { get; set; }
        public EducationalInstitutionData EducationalInstitution { get; set; }
        public PersonTechnical ResourceTargetPerson { get; set; }

        public class ApplicationSocialStatusData
        {
            public Guid Id { get; set; }
            public ClassifierData SocialStatus { get; set; }
            public bool? SocialStatusApproved { get; set; }
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
            public IEnumerable<PersonData> Persons { get; set; }
            public IEnumerable<ContactData> ContactInformation { get; set; }

            public class PersonData
            {
                public Guid Id { get; set; }
                public DateTime ActiveFrom { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string PrivatePersonalIdentifier { get; set; }
            }

            public class ContactData
            {
                public ClassifierData Type { get; set; }
                public string Value { get; set; }
            }
        }

        public class PersonTechnical
        {
            public Guid Id { get; set; }
            public IEnumerable<PersonData> Persons { get; set; }

            public class PersonData
            {
                public Guid Id { get; set; }
                public DateTime ActiveFrom { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string PrivatePersonalIdentifier { get; set; }
            }
        }

        public class ApplicationResourceData
        {
            public Guid Id { get; set; }
            public DateTime? AssignedResourceReturnDate { get; set; }
            public IEnumerable<ApplicationResourceAttachmentData> Attachments { get; set; }
            public IEnumerable<EducationalInstitutionContactPersonData> EducationalInstitutionContactPersons { get; set; }
            public string PNANumber { get; set; }
            public ClassifierData PNAStatus { get; set; }

            public class ResourceData
            {
                public Guid Id { get; set; }
                public decimal AcquisitionsValue { get; set; }
                public string InventoryNumber { get; set; }
                public ClassifierData Manufacturer { get; set; }
                public string ModelIdentifier { get; set; }
                public ClassifierData ModelName { get; set; }
                public string Notes { get; set; }
                public ClassifierData ResourceStatus { get; set; }
                public string SerialNumber { get; set; }
            }

            public class ApplicationResourceAttachmentData
            {
                public Guid Id { get; set; }
                public DateTime DocumentDate { get; set; }
                public ClassifierData DocumentType { get; set; }
                public FileData File { get; set; }

                public class FileData
                {
                    public string ContentType { get; set; }
                    public string Extension { get; set; }
                    public string Name { get; set; }
                }
            }

            public class EducationalInstitutionContactPersonData
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
                public string Email { get; set; }
                public string PhoneNumber { get; set; }
                public ClassifierData JobPosition { get; set; }
            }
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
    }

    public class EServiceApplicationIntermediateResponse : EServiceApplication
    {
        public IEnumerable<ApplicationResourceDataExpanded> ApplicationResources { get; set; }
        public ClassifierDataWithPayload ResourceSubType { get; set; }

        public class ApplicationResourceDataExpanded : ApplicationResourceData
        {
            public DateTime Created { get; set; }
            public ResourceDataExpanded Resource { get; set; }
            public class ResourceDataExpanded : ResourceData
            {
                public ClassifierDataWithPayload ResourceSubType { get; set; }
            }
        }
    }

    public class EServiceApplicationResponse : EServiceApplication
    {
        public IEnumerable<ApplicationResourceDataExpanded> ApplicationResources { get; set; }
        public ClassifierData ResourceSubType { get; set; }
        public ClassifierData ResourceType { get; set; }

        public class ApplicationResourceDataExpanded : ApplicationResourceData
        {
            public ResourceDataExpanded Resource { get; set; }
            public class ResourceDataExpanded : ResourceData
            {
                public ClassifierData ResourceSubType { get; set; }
                public ClassifierData ResourceType { get; set; }
            }
        }
    }

    public class EServiceEducationalInstitutionData
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public ClassifierData Status { get; set; }

        public class ClassifierData
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Value { get; set; }
        }
    }

    public class EServiceApplicationResponseIntermediateData
    {
        public Guid Id { get; set; }
        public DateTime ApplicationDate { get; set; }
        public ClassifierData ResourceSubType { get; set; }
        public PersonTechnical ResourceTargetPerson { get; set; }
        public EducationalInstitutionData EducationalInstitution { get; set; }
        public ClassifierDataWithType ApplicationStatus { get; set; }
        public IEnumerable<ApplicationResourceData> ApplicationResources { get; set; } = Array.Empty<ApplicationResourceData>();

        public class ClassifierData
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Value { get; set; }
        }

        public class ClassifierDataWithType : ClassifierData
        {
            public string Type { get; set; }
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

        public class EducationalInstitutionData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class ApplicationResourceData
        {
            public DateTime Created { get; set; }
            public DateOnly? PnaDate { get; set; }
            public ClassifierDataWithType Status { get; set; }
        }
    }

    public class EServiceApplicationResponseData
    {
        public Guid Id { get; set; }
        public DateTime ApplicationDate { get; set; }
        public ClassifierData ResourceSubType { get; set; }
        public PersonTechnical ResourceTargetPerson { get; set; }
        public EducationalInstitutionData EducationalInstitution { get; set; }
        public ClassifierDataWithType ApplicationStatus { get; set; }
        public DateTime? PNADate { get; set; }

        public class ClassifierData
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Value { get; set; }
        }

        public class ClassifierDataWithType : ClassifierData
        {
            public string Type { get; set; }
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

        public class EducationalInstitutionData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public static Func<EServiceApplicationResponseIntermediateData, EServiceApplicationResponseData> Project()
        {
            var validPnaStatuses = new HashSet<string> { PnaStatus.Prepared, PnaStatus.Issued, PnaStatus.Returned, PnaStatus.Stolen, PnaStatus.Lost };

            return t => new EServiceApplicationResponseData
            {
                Id = t.Id,
                ApplicationDate = t.ApplicationDate,
                ResourceSubType = new ClassifierData
                {
                    Id = t.ResourceSubType.Id,
                    Value = t.ResourceSubType.Value,
                    Code = t.ResourceSubType.Code,
                },
                ResourceTargetPerson = new PersonTechnical
                {
                    Id = t.ResourceTargetPerson.Id,
                    Person = t.ResourceTargetPerson.Person.Select(t => new PersonTechnical.PersonData
                    {
                        Id = t.Id,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        PrivatePersonalIdentifier = t.PrivatePersonalIdentifier
                    })
                },
                EducationalInstitution = new EducationalInstitutionData
                {
                    Id = t.EducationalInstitution.Id,
                    Code = t.EducationalInstitution.Code,
                    Name = t.EducationalInstitution.Name,
                },
                ApplicationStatus = validPnaStatuses.Contains(t.ApplicationResources.OrderBy(n => n.Created).Select(n => n.Status.Code).LastOrDefault())
                    ? t.ApplicationResources
                        .OrderBy(n => n.Created)
                        .Select(n => new ClassifierDataWithType
                        {
                            Id = n.Status.Id,
                            Type = n.Status.Type,
                            Code = n.Status.Code,
                            Value = n.Status.Value,
                        }).LastOrDefault()
                    : new ClassifierDataWithType
                    {
                        Id = t.ApplicationStatus.Id,
                        Type = t.ApplicationStatus.Type,
                        Code = t.ApplicationStatus.Code,
                        Value = t.ApplicationStatus.Value,
                    },
                PNADate = t.ApplicationResources.Any(n => PnaStatus.ActiveStatuses.Contains(n.Status.Code))
                    ? t.ApplicationResources
                        .Where(n => PnaStatus.ActiveStatuses.Contains(n.Status.Code))
                        .Select(n => n.PnaDate)
                        .FirstOrDefault()?.ToDateTime(TimeOnly.MinValue)
                    : t.ApplicationResources
                        .Where(n => PnaStatus.NonActiveStatuses.Contains(n.Status.Code))
                        .OrderBy(n => n.Created)
                        .Select(n => n.PnaDate)
                        .LastOrDefault()?.ToDateTime(TimeOnly.MinValue)
            };
        }
    }

    public class EServiceApplicationCheckDuplicateRequest
    {
        public string PrivatePersonalIdentifier { get; set; }
        public Guid ResourceSubTypeId { get; set; }
    }

    public class EServiceApplicationCheckDuplicateResponse
    {
        public EducationalInstitutionData EducationalInstitution { get; set; }

        public class EducationalInstitutionData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }

    public class EServiceClassifierResponse
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public int? SortOrder { get; set; }
        public string Payload { get; set; }
    }

    public class EServiceContactPersonUpdateRequest
    {
        public IEnumerable<ContactInformation> ApplicationContactInformation { get; set; } = Array.Empty<ContactInformation>();

        public class ContactInformation
        {
            public Guid TypeId { get; set; }
            public string Value { get; set; }
        }
    }

    public class EServiceApplicationsContactPersonUpdateRequest
    {
        public Guid ResourceTargetPersonId { get; set; }
        public ContactPersonData ContactPerson { get; set; }

        public class ContactPersonData
        {
            public IEnumerable<ContactInformation> ApplicationContactInformation { get; set; } = Array.Empty<ContactInformation>();

            public class ContactInformation
            {
                public Guid TypeId { get; set; }
                public string Value { get; set; }
            }
        }
    }

    public class EServiceChangeStatusRequest
    {
        public IEnumerable<Guid> FileToDeleteIds { get; set; }
        public IEnumerable<Base64File> Files { get; set; } = Array.Empty<Base64File>();
        public string Notes { get; set; }

        public class Base64File
        {
            public string Content { get; set; }
            public string ContentType { get; set; }
            public string FileName { get; set; }
        }
    }

    public class EServiceDocumentTemplatesRequest
    {
        public Guid ResourceTypeId { get; set; }
        public int EducationalInstitutionId { get; set; }
    }

    public class EServiceDocumentTemplateResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Hyperlink { get; set; }
    }
}