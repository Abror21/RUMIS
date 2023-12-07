using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Application.Models.Application;
using Izm.Rumis.Domain.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    public class ApplicationMapper
    {
        public static ApplicationCreateResponse Map(ApplicationCreateResult result, ApplicationCreateResponse model)
        {
            model.Id = result.Id;
            model.ApplicationNumber = result.Number;

            return model;
        }

        public static ApplicationCreateDto Map(ApplicationCreateRequest model, ApplicationCreateDto dto)
        {
            dto.ApplicationSocialStatuses = model.ApplicationSocialStatuses;
            dto.ApplicationStatusHistory = model.ApplicationStatusHistory;
            dto.EducationalInstitutionId = model.EducationalInstitutionId;
            dto.Notes = model.Notes;
            dto.ResourceSubTypeId = model.ResourceSubTypeId;
            dto.ResourceTargetPersonClassGrade = model.ResourceTargetPersonClassGrade;
            dto.ResourceTargetPersonClassParallel = model.ResourceTargetPersonClassParallel;
            dto.ResourceTargetPersonEducationalProgram = model.ResourceTargetPersonEducationalProgram;
            dto.ResourceTargetPersonEducationalStatusId = model.ResourceTargetPersonEducationalStatusId;
            dto.ResourceTargetPersonEducationalSubStatusId = model.ResourceTargetPersonEducationalSubStatusId;
            dto.ResourceTargetPersonGroup = model.ResourceTargetPersonGroup;
            dto.ResourceTargetPerson = model.ResourceTargetPerson == null ? null : new PersonData
            {
                ContactInformation = model.ResourceTargetPerson.ContactInformation.Select(t => new PersonData.ContactData
                {
                    TypeId = t.TypeId,
                    Value = t.Value
                }),
                FirstName = model.ResourceTargetPerson.FirstName,
                LastName = model.ResourceTargetPerson.LastName,
                PrivatePersonalIdentifier = model.ResourceTargetPerson.PrivatePersonalIdentifier
            };
            dto.ResourceTargetPersonTypeId = model.ResourceTargetPersonTypeId;
            dto.ResourceTargetPersonWorkStatusId = model.ResourceTargetPersonWorkStatusId;
            dto.SocialStatus = model.SocialStatus;
            dto.SocialStatusApproved = model.SocialStatusApproved;
            dto.SubmitterPerson = model.SubmitterPerson == null ? null : new PersonData
            {
                ContactInformation = model.SubmitterPerson.ContactInformation.Select(t => new PersonData.ContactData
                {
                    TypeId = t.TypeId,
                    Value = t.Value
                }),
                FirstName = model.SubmitterPerson.FirstName,
                LastName = model.SubmitterPerson.LastName,
                PrivatePersonalIdentifier = model.SubmitterPerson.PrivatePersonalIdentifier
            };
            dto.SubmitterTypeId = model.SubmitterTypeId;

            return dto;
        }

        public static ApplicationUpdateDto Map(ApplicationUpdateRequest model, ApplicationUpdateDto dto)
        {
            dto.ApplicationStatusHistory = model.ApplicationStatusHistory;
            dto.ApplicationStatusId = model.ApplicationStatusId;
            dto.ContactPersonId = model.ContactPersonId;
            dto.Notes = model.Notes;
            dto.ResourceSubTypeId = model.ResourceSubTypeId;
            dto.ResourceTargetPersonClassGrade = model.ResourceTargetPersonClassGrade;
            dto.ResourceTargetPersonClassParallel = model.ResourceTargetPersonClassParallel;
            dto.ResourceTargetPersonEducationalProgram = model.ResourceTargetPersonEducationalProgram;
            dto.ResourceTargetPersonEducationalStatusId = model.ResourceTargetPersonEducationalStatusId;
            dto.ResourceTargetPersonEducationalSubStatusId = model.ResourceTargetPersonEducationalSubStatusId;
            dto.ResourceTargetPersonGroup = model.ResourceTargetPersonGroup;
            dto.ResourceTargetPersonId = model.ResourceTargetPersonId;
            dto.ResourceTargetPersonTypeId = model.ResourceTargetPersonTypeId;
            dto.ResourceTargetPersonWorkStatusId = model.ResourceTargetPersonWorkStatusId;
            dto.SocialStatus = model.SocialStatus;
            dto.SocialStatusApproved = model.SocialStatusApproved;
            dto.SubmitterTypeId = model.SubmitterTypeId;

            return dto;
        }

        public static ApplicationDeclineDto Map(ApplicationDeclineRequest model, ApplicationDeclineDto dto)
        {
            dto.ApplicationIds = model.ApplicationIds;
            dto.Reason = model.Reason;

            return dto;
        }

        public static ApplicationSocialStatusResponse Map(Domain.Entities.Application entity, ApplicationSocialStatusResponse model)
        {
            model.Id = entity.Id;
            model.SocialStatusApproved = entity.SocialStatusApproved;
            model.ApplicationSocialStatus = entity.ApplicationSocialStatuses.Select(t => new ApplicationSocialStatusResponse.ApplicationSocialStatusData
            {
                Id = t.Id,
                SocialStatus = new ApplicationSocialStatusResponse.ApplicationSocialStatusData.ClassifierData
                {
                    Id = t.SocialStatus.Id,
                    Code = t.SocialStatus.Code,
                    Value = t.SocialStatus.Value
                },
                SocialStatusApproved = t.SocialStatusApproved
            });

            return model;
        }

        public static Expression<Func<Domain.Entities.Application, ApplicationResponse>> Project()
        {
            return t => new ApplicationResponse
            {
                ApplicationDate = t.ApplicationDate,
                ApplicationNumber = t.ApplicationNumber,
                ApplicationResources = t.ApplicationResources.Select(t => new ApplicationResponse.ApplicationResourceData
                {
                    Id = t.Id,
                    PNANumber = t.PNANumber,
                    PNAStatus = new ApplicationResponse.ClassifierData
                    {
                        Id = t.PNAStatusId,
                        Code = t.PNAStatus.Code,
                        Value = t.PNAStatus.Value
                    }
                }),
                ApplicationSocialStatus = t.ApplicationSocialStatuses.Select(t => new ApplicationResponse.ApplicationSocialStatusData
                {
                    Id = t.Id,
                    SocialStatus = new ApplicationResponse.ClassifierData
                    {
                        Id = t.SocialStatusId,
                        Code = t.SocialStatus.Code,
                        Value = t.SocialStatus.Value
                    },
                    SocialStatusApproved = t.SocialStatusApproved
                }),
                ApplicationStatus = new ApplicationResponse.ClassifierData
                {
                    Id = t.ApplicationStatusId,
                    Code = t.ApplicationStatus.Code,
                    Value = t.ApplicationStatus.Value
                },
                ApplicationStatusHistory = t.ApplicationStatusHistory,
                ContactPerson = new ApplicationResponse.PersonTechnicalWithContactData
                {
                    Id = t.ContactPersonId,
                    Person = t.ContactPerson.Persons.Select(t => new ApplicationResponse.PersonTechnicalWithContactData.PersonData
                    {
                        Id = t.Id,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        PrivatePersonalIdentifier = t.PrivatePersonalIdentifier
                    }),
                    ContactData = t.ContactPerson.PersonContacts
                    .Where(n => n.IsActive == true)
                    .Select(n => new ApplicationResponse.PersonTechnicalWithContactData.ContactInformation
                    {
                        Type = new ApplicationResponse.ClassifierData
                        {
                            Code = n.ContactType.Code,
                            Id = n.ContactType.Id,
                            Value = n.ContactType.Value
                        },
                        Value = n.ContactValue
                    }),
                },
                Created = t.Created,
                CreatedById = t.CreatedById,
                EducationalInstitution = new ApplicationResponse.EducationalInstitutionData
                {
                    Id = t.EducationalInstitutionId,
                    Code = t.EducationalInstitution.Code,
                    Name = t.EducationalInstitution.Name
                },
                Modified = t.Modified,
                ModifiedById = t.ModifiedById,
                MonitoringClassGrade = t.MonitoringClassGrade,
                MonitoringClassParallel = t.MonitoringClassParallel,
                MonitoringEducationalStatus = t.MonitoringEducationalStatus == null ? null : new ApplicationResponse.ClassifierData
                {
                    Id = t.MonitoringEducationalStatusId.Value,
                    Code = t.MonitoringEducationalStatus.Code,
                    Value = t.MonitoringEducationalStatus.Value
                },
                MonitoringEducationalSubStatus = t.MonitoringEducationalSubStatus == null ? null : new ApplicationResponse.ClassifierData
                {
                    Id = t.MonitoringEducationalSubStatusId.Value,
                    Code = t.MonitoringEducationalSubStatus.Code,
                    Value = t.MonitoringEducationalSubStatus.Value
                },
                MonitoringGroup = t.MonitoringGroup,
                MonitoringWorkStatus = t.MonitoringWorkStatus == null ? null : new ApplicationResponse.ClassifierData
                {
                    Id = t.MonitoringWorkStatusId.Value,
                    Code = t.MonitoringWorkStatus.Code,
                    Value = t.MonitoringWorkStatus.Value
                },
                Notes = t.Notes,
                ResourceSubType = new ApplicationResponse.ClassifierData
                {
                    Id = t.ResourceSubTypeId,
                    Code = t.ResourceSubType.Code,
                    Value = t.ResourceSubType.Value
                },
                ResourceTargetPerson = new ApplicationResponse.PersonTechnical
                {
                    Id = t.ResourceTargetPersonId,
                    Person = t.ResourceTargetPerson.Persons.Select(t => new ApplicationResponse.PersonTechnical.PersonData
                    {
                        Id = t.Id,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        PrivatePersonalIdentifier = t.PrivatePersonalIdentifier
                    })
                },
                ResourceTargetPersonClassGrade = t.ResourceTargetPersonClassGrade,
                ResourceTargetPersonClassParallel = t.ResourceTargetPersonClassParallel,
                ResourceTargetPersonEducationalProgram = t.ResourceTargetPersonEducationalProgram,
                ResourceTargetPersonEducationalStatus = t.ResourceTargetPersonEducationalStatus == null ? null : new ApplicationResponse.ClassifierData
                {
                    Id = t.ResourceTargetPersonEducationalStatus.Id,
                    Code = t.ResourceTargetPersonEducationalStatus.Code,
                    Value = t.ResourceTargetPersonEducationalStatus.Value
                },
                ResourceTargetPersonEducationalSubStatus = t.ResourceTargetPersonEducationalSubStatus == null ? null : new ApplicationResponse.ClassifierData
                {
                    Id = t.ResourceTargetPersonEducationalSubStatus.Id,
                    Code = t.ResourceTargetPersonEducationalSubStatus.Code,
                    Value = t.ResourceTargetPersonEducationalSubStatus.Value
                },
                ResourceTargetPersonGroup = t.ResourceTargetPersonGroup,
                ResourceTargetPersonType = new ApplicationResponse.ClassifierData
                {
                    Id = t.ResourceTargetPersonTypeId,
                    Code = t.ResourceTargetPersonType.Code,
                    Value = t.ResourceTargetPersonType.Value
                },
                ResourceTargetPersonWorkStatus = t.ResourceTargetPersonWorkStatus == null ? null : new ApplicationResponse.ClassifierData
                {
                    Id = t.ResourceTargetPersonWorkStatus.Id,
                    Code = t.ResourceTargetPersonWorkStatus.Code,
                    Value = t.ResourceTargetPersonWorkStatus.Value
                },
                SocialStatus = t.SocialStatus,
                SocialStatusApproved = t.SocialStatusApproved,
                SubmitterPerson = new ApplicationResponse.PersonTechnical
                {
                    Id = t.SubmitterPersonId,
                    Person = t.SubmitterPerson.Persons.Select(t => new ApplicationResponse.PersonTechnical.PersonData
                    {
                        Id = t.Id,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        PrivatePersonalIdentifier = t.PrivatePersonalIdentifier
                    })
                },
                SubmitterType = new ApplicationResponse.ClassifierData
                {
                    Id = t.SubmitterTypeId,
                    Code = t.SubmitterType.Code,
                    Value = t.SubmitterType.Value
                }
            };
        }

        public static Expression<Func<Domain.Entities.Application, ApplicationIntermediateListItemResponse>> ProjectIntermediateListItem()
        {
            return t => new ApplicationIntermediateListItemResponse
            {
                ApplicationDate = t.ApplicationDate,
                ApplicationNumber = t.ApplicationNumber,
                ApplicationSocialStatus = t.ApplicationSocialStatuses.Select(t => new ApplicationListItem.ApplicationSocialStatusData
                {
                    Id = t.Id,
                    SocialStatus = new ApplicationListItem.ClassifierData
                    {
                        Id = t.SocialStatus.Id,
                        Code = t.SocialStatus.Code,
                        Value = t.SocialStatus.Value
                    },
                }),
                ApplicationStatus = new ApplicationListItem.ClassifierData
                {
                    Id = t.ApplicationStatusId,
                    Code = t.ApplicationStatus.Code,
                    Value = t.ApplicationStatus.Value
                },
                ApplicationStatusHistory = t.ApplicationStatusHistory,
                ContactPerson = new ApplicationListItem.ContactPersonTechnical
                {
                    Id = t.ContactPersonId,
                    Person = t.ContactPerson.Persons.Select(t => new ApplicationListItem.PersonTechnical.PersonData
                    {
                        Id = t.Id,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        PrivatePersonalIdentifier = t.PrivatePersonalIdentifier
                    }),
                    Contacts = t.ContactPerson.PersonContacts.Where(t => t.IsActive).Select(t => new ApplicationListItem.ContactPersonTechnical.ContactData
                    {
                        Id = t.Id,
                        ContactValue = t.ContactValue,
                        ContactType = new ApplicationListItem.ClassifierData
                        {
                            Id = t.ContactType.Id,
                            Code = t.ContactType.Code,
                            Value = t.ContactType.Value
                        }
                    })
                },
                Created = t.Created,
                CreatedById = t.CreatedById,
                EducationalInstitution = new ApplicationListItem.EducationalInstitutionData
                {
                    Id = t.EducationalInstitutionId,
                    Code = t.EducationalInstitution.Code,
                    Name = t.EducationalInstitution.Name
                },
                HasDuplicate = t.ApplicationDuplicateId.HasValue,
                Id = t.Id,
                Modified = t.Modified,
                ModifiedById = t.ModifiedById,
                MonitoringClassGrade = t.MonitoringClassGrade,
                MonitoringClassParallel = t.MonitoringClassParallel,
                MonitoringEducationalStatus = t.MonitoringEducationalStatus == null ? null : new ApplicationListItem.ClassifierData
                {
                    Id = t.MonitoringEducationalStatusId.Value,
                    Code = t.MonitoringEducationalStatus.Code,
                    Value = t.MonitoringEducationalStatus.Value
                },
                MonitoringEducationalSubStatus = t.MonitoringEducationalSubStatus == null ? null : new ApplicationListItem.ClassifierData
                {
                    Id = t.MonitoringEducationalSubStatusId.Value,
                    Code = t.MonitoringEducationalSubStatus.Code,
                    Value = t.MonitoringEducationalSubStatus.Value
                },
                MonitoringGroup = t.MonitoringGroup,
                MonitoringWorkStatus = t.MonitoringWorkStatus == null ? null : new ApplicationListItem.ClassifierData
                {
                    Id = t.MonitoringWorkStatusId.Value,
                    Code = t.MonitoringWorkStatus.Code,
                    Value = t.MonitoringWorkStatus.Value
                },
                Notes = t.Notes,
                ResourceSubType = new ApplicationListItem.ClassifierDataWithPayload
                {
                    Id = t.ResourceSubTypeId,
                    Code = t.ResourceSubType.Code,
                    Payload = t.ResourceSubType.Payload,
                    Value = t.ResourceSubType.Value,
                },
                ResourceTargetPerson = new ApplicationListItem.PersonTechnical
                {
                    Id = t.ResourceTargetPersonId,
                    Person = t.ResourceTargetPerson.Persons.Select(t => new ApplicationListItem.PersonTechnical.PersonData
                    {
                        Id = t.Id,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        PrivatePersonalIdentifier = t.PrivatePersonalIdentifier
                    })
                },
                ResourceTargetPersonClassGrade = t.ResourceTargetPersonClassGrade,
                ResourceTargetPersonClassParallel = t.ResourceTargetPersonClassParallel,
                ResourceTargetPersonEducationalProgram = t.ResourceTargetPersonEducationalProgram,
                ResourceTargetPersonEducationalStatus = t.ResourceTargetPersonEducationalStatus == null ? null : new ApplicationListItem.ClassifierData
                {
                    Id = t.ResourceTargetPersonEducationalStatus.Id,
                    Code = t.ResourceTargetPersonEducationalStatus.Code,
                    Value = t.ResourceTargetPersonEducationalStatus.Value
                },
                ResourceTargetPersonEducationalSubStatus = t.ResourceTargetPersonEducationalSubStatus == null ? null : new ApplicationListItem.ClassifierData
                {
                    Id = t.ResourceTargetPersonEducationalSubStatus.Id,
                    Code = t.ResourceTargetPersonEducationalSubStatus.Code,
                    Value = t.ResourceTargetPersonEducationalSubStatus.Value
                },
                ResourceTargetPersonGroup = t.ResourceTargetPersonGroup,
                ResourceTargetPersonType = new ApplicationListItem.ClassifierData
                {
                    Id = t.ResourceTargetPersonTypeId,
                    Code = t.ResourceTargetPersonType.Code,
                    Value = t.ResourceTargetPersonType.Value
                },
                ResourceTargetPersonWorkStatus = t.ResourceTargetPersonWorkStatus == null ? null : new ApplicationListItem.ClassifierData
                {
                    Id = t.ResourceTargetPersonWorkStatus.Id,
                    Code = t.ResourceTargetPersonWorkStatus.Code,
                    Value = t.ResourceTargetPersonWorkStatus.Value
                },
                SocialStatus = t.SocialStatus,
                SocialStatusApproved = t.SocialStatusApproved,
                SubmitterPerson = new ApplicationListItem.PersonTechnical
                {
                    Id = t.SubmitterPersonId,
                    Person = t.SubmitterPerson.Persons.Select(t => new ApplicationListItem.PersonTechnical.PersonData
                    {
                        Id = t.Id,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        PrivatePersonalIdentifier = t.PrivatePersonalIdentifier
                    })
                },
                SubmitterType = new ApplicationListItem.ClassifierData
                {
                    Id = t.SubmitterTypeId,
                    Code = t.SubmitterType.Code,
                    Value = t.SubmitterType.Value
                },
                Supervisor = new ApplicationListItem.SupervisorData
                {
                    Id = t.EducationalInstitution.Supervisor.Id,
                    Code = t.EducationalInstitution.Supervisor.Code,
                    Name = t.EducationalInstitution.Supervisor.Name
                }
            };
        }

        public static ApplicationListItemResponse Map(ApplicationIntermediateListItemResponse intermediate, ApplicationListItemResponse response)
        {
            response.ApplicationDate = intermediate.ApplicationDate;
            response.ApplicationNumber = intermediate.ApplicationNumber;
            response.ApplicationSocialStatus = intermediate.ApplicationSocialStatus;
            response.ApplicationStatus = intermediate.ApplicationStatus;
            response.ApplicationStatusHistory = intermediate.ApplicationStatusHistory;
            response.ContactPerson = intermediate.ContactPerson;
            response.Created = intermediate.Created;
            response.CreatedById = intermediate.CreatedById;
            response.EducationalInstitution = intermediate.EducationalInstitution;
            response.HasDuplicate = intermediate.HasDuplicate;
            response.Id = intermediate.Id;
            response.Modified = intermediate.Modified;
            response.ModifiedById = intermediate.ModifiedById;
            response.MonitoringClassGrade = intermediate.MonitoringClassGrade;
            response.MonitoringClassParallel = intermediate.MonitoringClassParallel;
            response.MonitoringEducationalStatus = intermediate.MonitoringEducationalStatus;
            response.MonitoringEducationalSubStatus = intermediate.MonitoringEducationalSubStatus;
            response.MonitoringGroup = intermediate.MonitoringGroup;
            response.MonitoringWorkStatus = intermediate.MonitoringWorkStatus;
            response.Notes = intermediate.Notes;
            response.ResourceSubType = new ApplicationListItem.ClassifierData
            {
                Id = intermediate.ResourceSubType.Id,
                Code = intermediate.ResourceSubType.Code,
                Value = intermediate.ResourceSubType.Value
            };
            response.ResourceTargetPerson = intermediate.ResourceTargetPerson;
            response.ResourceTargetPersonClassGrade = intermediate.ResourceTargetPersonClassGrade;
            response.ResourceTargetPersonClassParallel = intermediate.ResourceTargetPersonClassParallel;
            response.ResourceTargetPersonEducationalProgram = intermediate.ResourceTargetPersonEducationalProgram;
            response.ResourceTargetPersonEducationalStatus = intermediate.ResourceTargetPersonEducationalStatus;
            response.ResourceTargetPersonEducationalSubStatus = intermediate.ResourceTargetPersonEducationalSubStatus;
            response.ResourceTargetPersonGroup = intermediate.ResourceTargetPersonGroup;
            response.ResourceTargetPersonType = intermediate.ResourceTargetPersonType;
            response.ResourceTargetPersonWorkStatus = intermediate.ResourceTargetPersonWorkStatus;
            response.SocialStatus = intermediate.SocialStatus;
            response.SocialStatusApproved = intermediate.SocialStatusApproved;
            response.SubmitterPerson = intermediate.SubmitterPerson;
            response.SubmitterType = intermediate.SubmitterType;
            response.Supervisor = intermediate.Supervisor;

            return response;
        }

        public static Expression<Func<Classifier, ApplicationListItem.ClassifierData>> ProjectApplicationListItemClassifier()
        {
            return t => new ApplicationListItem.ClassifierData
            {
                Id = t.Id,
                Code = t.Code,
                Value = t.Value
            };
        }

        public static ApplicationContactInformationUpdateDto Map(ApplicationContactPersonUpdateRequest model, ApplicationContactInformationUpdateDto dto)
        {
            dto.Person = new PersonData
            {
                ContactInformation = model.ApplicationContactInformation.Select(t => new PersonData.ContactData
                {
                    TypeId = t.TypeId,
                    Value = t.Value
                }).ToArray(),
                PrivatePersonalIdentifier = model.PrivatePersonalIdentifier,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            return dto;
        }

        public static ApplicationsContactInformationUpdateDto Map(ApplicationsContactPersonUpdateRequest model, ApplicationsContactInformationUpdateDto dto)
        {
            dto.Person = new PersonData
            {
                ContactInformation = model.ContactPerson.ApplicationContactInformation.Select(t => new PersonData.ContactData
                {
                    TypeId = t.TypeId,
                    Value = t.Value
                }).ToArray(),
                PrivatePersonalIdentifier = model.ContactPerson.PrivatePersonalIdentifier,
                FirstName = model.ContactPerson.FirstName,
                LastName = model.ContactPerson.LastName
            };

            return dto;
        }

        public static ApplicationCheckDuplicateDto Map(ApplicationCheckDuplicateRequest model, ApplicationCheckDuplicateDto dto)
        {
            dto.PrivatePersonalIdentifier = model.PrivatePersonalIdentifier;
            dto.ResourceSubTypeId = model.ResourceSubTypeId;

            return dto;
        }

        public static Expression<Func<Domain.Entities.Application, ApplicationCheckDuplicateResponse>> ProjectDuplicate()
        {
            return t => new ApplicationCheckDuplicateResponse
            {
                EducationalInstitution = new ApplicationCheckDuplicateResponse.EducationalInstitutionData
                {
                    Id = t.EducationalInstitutionId,
                    Code = t.EducationalInstitution.Code,
                    Name = t.EducationalInstitution.Name
                }
            };
        }
    }
}
