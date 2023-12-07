using Izm.Rumis.Api.Models;
using Izm.Rumis.Application;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Infrastructure.EServices.Dtos;
using Izm.Rumis.Infrastructure.EServices.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    internal static class EServiceMapper
    {
        public static EServiceApplicationCreateResponse Map(EServiceApplicationCreateResult result, EServiceApplicationCreateResponse model)
        {
            model.ApplicationNumber = result.Number;
            model.Id = result.Id;

            return model;
        }

        public static Expression<Func<Domain.Entities.Application, EServiceApplicationIntermediateResponse>> ProjectIntermediateApplication()
        {
            return t => new EServiceApplicationIntermediateResponse
            {
                ApplicationDate = t.ApplicationDate,
                ApplicationNumber = t.ApplicationNumber,
                ApplicationResources = t.ApplicationResources.Select(ar => new EServiceApplicationIntermediateResponse.ApplicationResourceDataExpanded
                {
                    Id = ar.Id,
                    AssignedResourceReturnDate = ar.AssignedResourceReturnDate,
                    Attachments = ar.ApplicationResourceAttachmentList.Select(a => new EServiceApplication.ApplicationResourceData.ApplicationResourceAttachmentData
                    {
                        Id = a.Id,
                        DocumentDate = a.DocumentDate.ToDateTime(new TimeOnly()),
                        DocumentType = a.DocumentType == null ? null : new EServiceApplication.ClassifierData
                        {
                            Id = a.DocumentType.Id,
                            Code = a.DocumentType.Code,
                            Value = a.DocumentType.Value
                        },
                        File = a.File == null ? null : new EServiceApplication.ApplicationResourceData.ApplicationResourceAttachmentData.FileData
                        {
                            ContentType = a.File.ContentType,
                            Extension = a.File.Extension,
                            Name = a.File.Name
                        }
                    }),
                    EducationalInstitutionContactPersons = ar.ApplicationResourceContactPersons.Select(c => new EServiceApplication.ApplicationResourceData.EducationalInstitutionContactPersonData
                    {
                        Id = c.EducationalInstitutionContactPerson.Id,
                        Name = c.EducationalInstitutionContactPerson.Name,
                        Email = c.EducationalInstitutionContactPerson.Email,
                        PhoneNumber = c.EducationalInstitutionContactPerson.PhoneNumber,
                        JobPosition = new EServiceApplication.ClassifierData
                        {
                            Id = c.EducationalInstitutionContactPerson.JobPosition.Id,
                            Code = c.EducationalInstitutionContactPerson.JobPosition.Code,
                            Value = c.EducationalInstitutionContactPerson.JobPosition.Value
                        }
                    }),
                    PNANumber = ar.PNANumber,
                    PNAStatus = new EServiceApplication.ClassifierData
                    {
                        Id = ar.PNAStatusId,
                        Code = ar.PNAStatus.Code,
                        Value = ar.PNAStatus.Value
                    },
                    Resource = ar.AssignedResourceId == null ? null : new EServiceApplicationIntermediateResponse.ApplicationResourceDataExpanded.ResourceDataExpanded
                    {
                        Id = ar.AssignedResourceId.Value,
                        AcquisitionsValue = ar.AssignedResource.AcquisitionsValue,
                        InventoryNumber = ar.AssignedResource.InventoryNumber,
                        Manufacturer = new EServiceApplication.ClassifierData
                        {
                            Id = ar.AssignedResource.ManufacturerId,
                            Code = ar.AssignedResource.Manufacturer.Code,
                            Value = ar.AssignedResource.Manufacturer.Value
                        },
                        ModelIdentifier = ar.AssignedResource.ModelIdentifier,
                        ModelName = new EServiceApplication.ClassifierData
                        {
                            Id = ar.AssignedResource.ModelNameId,
                            Code = ar.AssignedResource.ModelName.Code,
                            Value = ar.AssignedResource.ModelName.Value
                        },
                        Notes = ar.AssignedResource.Notes,
                        ResourceStatus = new EServiceApplication.ClassifierData
                        {
                            Id = ar.AssignedResource.ResourceStatusId,
                            Code = ar.AssignedResource.ResourceStatus.Code,
                            Value = ar.AssignedResource.ResourceStatus.Value
                        },
                        ResourceSubType = new EServiceApplication.ClassifierDataWithPayload
                        {
                            Id = ar.AssignedResource.ResourceSubTypeId,
                            Code = ar.AssignedResource.ResourceSubType.Code,
                            Payload = ar.AssignedResource.ResourceSubType.Payload,
                            Value = ar.AssignedResource.ResourceSubType.Value
                        },
                        SerialNumber = ar.AssignedResource.SerialNumber
                    }
                }),
                ApplicationSocialStatuses = t.ApplicationSocialStatuses.Select(s => new EServiceApplication.ApplicationSocialStatusData
                {
                    Id = s.SocialStatusId,
                    SocialStatus = new EServiceApplication.ClassifierData
                    {
                        Id = s.SocialStatusId,
                        Code = s.SocialStatus.Code,
                        Value = s.SocialStatus.Value
                    },
                    SocialStatusApproved = s.SocialStatusApproved
                }),
                ApplicationStatus = new EServiceApplication.ClassifierData
                {
                    Id = t.ApplicationStatusId,
                    Code = t.ApplicationStatus.Code,
                    Value = t.ApplicationStatus.Value
                },
                ContactPerson = new EServiceApplication.PersonTechnicalWithContactData
                {
                    Id = t.ContactPersonId,
                    Persons = t.ContactPerson.Persons.Select(n => new EServiceApplication.PersonTechnicalWithContactData.PersonData
                    {
                        Id = n.Id,
                        ActiveFrom = n.ActiveFrom,
                        FirstName = n.FirstName,
                        LastName = n.LastName,
                        PrivatePersonalIdentifier = n.PrivatePersonalIdentifier
                    }),
                    ContactInformation = t.ContactPerson.PersonContacts
                    .Where(n => n.IsActive == true)
                    .Select(n => new EServiceApplication.PersonTechnicalWithContactData.ContactData
                    {
                        Type = new EServiceApplication.ClassifierData
                        {
                            Code = n.ContactType.Code,
                            Id = n.ContactType.Id,
                            Value = n.ContactType.Value
                        },
                        Value = n.ContactValue
                    })
                },
                EducationalInstitution = new EServiceApplication.EducationalInstitutionData
                {
                    Id = t.EducationalInstitutionId,
                    Code = t.EducationalInstitution.Code,
                    Name = t.EducationalInstitution.Name
                },
                ResourceSubType = new EServiceApplication.ClassifierDataWithPayload
                {
                    Id = t.ResourceSubTypeId,
                    Code = t.ResourceSubType.Code,
                    Payload = t.ResourceSubType.Payload,
                    Value = t.ResourceSubType.Value
                },
                ResourceTargetPerson = new EServiceApplication.PersonTechnical
                {
                    Id = t.ResourceTargetPersonId,
                    Persons = t.ResourceTargetPerson.Persons.Select(n => new EServiceApplication.PersonTechnical.PersonData
                    {
                        Id = n.Id,
                        ActiveFrom = n.ActiveFrom,
                        FirstName = n.FirstName,
                        LastName = n.LastName,
                        PrivatePersonalIdentifier = n.PrivatePersonalIdentifier
                    })
                }
            };
        }

        public static Expression<Func<Domain.Entities.Application, EServiceApplicationResponseIntermediateData>> ProjectApplicationListItem()
        {
            return t => new EServiceApplicationResponseIntermediateData
            {
                Id = t.Id,
                ApplicationDate = t.ApplicationDate,
                ApplicationStatus = new EServiceApplicationResponseIntermediateData.ClassifierDataWithType
                {
                    Id = t.ApplicationStatusId,
                    Type = t.ApplicationStatus.Type,
                    Code = t.ApplicationStatus.Code,
                    Value = t.ApplicationStatus.Value
                },
                EducationalInstitution = new EServiceApplicationResponseIntermediateData.EducationalInstitutionData
                {
                    Id = t.EducationalInstitutionId,
                    Code = t.EducationalInstitution.Code,
                    Name = t.EducationalInstitution.Name
                },
                ResourceSubType = new EServiceApplicationResponseIntermediateData.ClassifierData
                {
                    Id = t.ResourceSubTypeId,
                    Code = t.ResourceSubType.Code,
                    Value = t.ResourceSubType.Value
                },
                ResourceTargetPerson = new EServiceApplicationResponseIntermediateData.PersonTechnical
                {
                    Id = t.ResourceTargetPersonId,
                    Person = t.ResourceTargetPerson.Persons.Select(t => new EServiceApplicationResponseIntermediateData.PersonTechnical.PersonData
                    {
                        Id = t.Id,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        PrivatePersonalIdentifier = t.PrivatePersonalIdentifier
                    })
                },
                ApplicationResources = t.ApplicationResources.Select(resource => new EServiceApplicationResponseIntermediateData.ApplicationResourceData
                {
                    Status = new EServiceApplicationResponseIntermediateData.ClassifierDataWithType
                    {
                        Id = resource.PNAStatus.Id,
                        Type = resource.PNAStatus.Type,
                        Code = resource.PNAStatus.Code,
                        Value = resource.PNAStatus.Value
                    },
                    PnaDate = resource.ApplicationResourceAttachmentList
                        .Where(ara => ara.DocumentType.Code == DocumentType.PNA)
                        .Select(ara => (DateOnly?)ara.DocumentDate)
                        .FirstOrDefault()

                })
            };
        }

        public static Func<EServiceEmployeeResponseDto, EServiceEmployeeResponse> ProjectEmployee()
        {
            return t => new EServiceEmployeeResponse
            {
                FirstName = t.FirstName,
                LastName = t.LastName,
                PrivatePersonalIdentifier = t.PrivatePersonalIdentifier,
                ActiveWorkData = t.ActiveWorkData == null ? null : t.ActiveWorkData.Select(inst => new EServiceEmployeeResponse.ActiveWorkDataResponse
                {
                    EducationInstitutionCode = inst.EducationInstitutionCode,
                    EducationInstitutionName = inst.EducationInstitutionName,
                    EducationalInstitutionId = inst.EducationalInstitutionId,
                    SupervisorName = inst.SupervisorName,
                    SupervisorCode = inst.SupervisorCode,
                    PositionName = inst.PositionName,
                    PositionCode = inst.PositionCode
                }).ToArray()
            };
        }

        public static Func<EServicesRelatedPersonResponseDto, EServiceRelatedPersonResponse> ProjectRelatedPerson()
        {
            return t => new EServiceRelatedPersonResponse
            {
                BirthDate = t.BirthDate,
                FirstName = t.FirstName,
                LastName = t.LastName,
                PrivatePersonalIdentifier = t.PrivatePersonalIdentifier,
                ActiveEducationData = t.ActiveEducationData == null ? null : t.ActiveEducationData.Select(inst => new EServiceRelatedPersonResponse.ActiveEducationDataResponse
                {
                    ClassGroup = inst.ClassGroup,
                    ClassGroupLevel = inst.ClassGroupLevel,
                    EducationProgram = inst.EducationProgram,
                    EducationProgramCode = inst.EducationProgramCode,
                    EducationInstitutionCode = inst.EducationInstitutionCode,
                    EducationInstitutionName = inst.EducationInstitutionName,
                    EducationalInstitutionId = inst.EducationalInstitutionId,
                    EducationInstitutionStatus = new EServiceRelatedPersonResponse.ActiveEducationDataResponse.ClassifierData
                    {
                        Code = inst.EducationInstitutionStatus.Code,
                        Id = inst.EducationInstitutionStatus.Id,
                        Value = inst.EducationInstitutionStatus.Value
                    },
                    SupervisorName = inst.SupervisorName,
                    SupervisorCode = inst.SupervisorCode
                }).ToArray()
            };
        }

        public static Expression<Func<Classifier, EServiceApplicationResponse.ClassifierData>> ProjectApplicationResponseClassifier()
        {
            return t => new EServiceApplicationResponse.ClassifierData
            {
                Code = t.Code,
                Id = t.Id,
                Value = t.Value
            };
        }

        public static EServiceApplicationCheckDuplicateDto Map(EServiceApplicationCheckDuplicateRequest model, EServiceApplicationCheckDuplicateDto dto)
        {
            dto.PrivatePersonalIdentifier = model.PrivatePersonalIdentifier;
            dto.ResourceSubTypeId = model.ResourceSubTypeId;

            return dto;
        }

        public static Expression<Func<Domain.Entities.Application, EServiceApplicationCheckDuplicateResponse>> ProjectApplicationDuplicate()
        {
            return t => new EServiceApplicationCheckDuplicateResponse
            {
                EducationalInstitution = new EServiceApplicationCheckDuplicateResponse.EducationalInstitutionData
                {
                    Id = t.EducationalInstitutionId,
                    Code = t.EducationalInstitution.Code,
                    Name = t.EducationalInstitution.Name
                }
            };
        }

        public static EServiceApplicationCreateDto Map(EServiceApplicationCreateRequest model, EServiceApplicationCreateDto dto)
        {
            dto.ApplicationSocialStatuses = model.ApplicationSocialStatuses;
            dto.ApplicationStatusHistory = model.ApplicationStatusHistory;
            dto.EducationalInstitutionId = model.EducationalInstitutionId;
            dto.Notes = model.Notes;
            dto.ResourceSubTypeId = model.ResourceSubTypeId;
            dto.ResourceTargetPerson = model.ResourceTargetPerson == null ? null : new EServiceApplicationCreateDto.PersonData
            {
                ContactInformation = model.ResourceTargetPerson.ContactInformation.Select(t => new EServiceApplicationCreateDto.PersonData.ContactData
                {
                    TypeId = t.TypeId,
                    Value = t.Value
                }).ToArray(),
                FirstName = model.ResourceTargetPerson.FirstName,
                LastName = model.ResourceTargetPerson.LastName,
                PrivatePersonalIdentifier = model.ResourceTargetPerson.PrivatePersonalIdentifier
            };
            dto.ResourceTargetPersonClassGrade = model.ResourceTargetPersonClassGrade;
            dto.ResourceTargetPersonClassParallel = model.ResourceTargetPersonClassParallel;
            dto.ResourceTargetPersonEducationalProgram = model.ResourceTargetPersonEducationalProgram;
            dto.ResourceTargetPersonEducationalStatusId = model.ResourceTargetPersonEducationalStatusId;
            dto.ResourceTargetPersonEducationalSubStatusId = model.ResourceTargetPersonEducationalSubStatusId;
            dto.ResourceTargetPersonGroup = model.ResourceTargetPersonGroup;
            dto.ResourceTargetPersonTypeId = model.ResourceTargetPersonTypeId;
            dto.ResourceTargetPersonWorkStatusId = model.ResourceTargetPersonWorkStatusId;
            dto.SocialStatus = model.SocialStatus;
            dto.SocialStatusApproved = model.SocialStatusApproved;
            dto.SubmitterContactData = model.SubmitterContactInformation.Select(t => new EServiceApplicationCreateDto.PersonData.ContactData
            {
                TypeId = t.TypeId,
                Value = t.Value
            }).ToArray();
            dto.SubmitterTypeId = model.SubmitterTypeId;

            return dto;
        }

        public static Expression<Func<Classifier, EServiceClassifierResponse>> ProjectClassifier()
        {
            return t => new EServiceClassifierResponse
            {
                Id = t.Id,
                Type = t.Type,
                Code = t.Code,
                Payload = t.Payload,
                Value = t.Value,
                SortOrder = t.SortOrder
            };
        }

        public static EServiceApplicationResponse Map(EServiceApplicationIntermediateResponse intermediate, EServiceApplicationResponse response)
        {
            response.ApplicationDate = intermediate.ApplicationDate;
            response.ApplicationNumber = intermediate.ApplicationNumber;
            response.ApplicationSocialStatuses = intermediate.ApplicationSocialStatuses;
            response.ApplicationStatus = intermediate.ApplicationStatus;
            response.EducationalInstitution = intermediate.EducationalInstitution;
            response.ResourceSubType = new EServiceApplication.ClassifierData
            {
                Id = intermediate.ResourceSubType.Id,
                Code = intermediate.ResourceSubType.Code,
                Value = intermediate.ResourceSubType.Value
            };
            response.ResourceTargetPerson = intermediate.ResourceTargetPerson;

            return response;
        }

        public static EServiceApplicationResponse MapContactPerson(EServiceApplicationIntermediateResponse intermediate, EServiceApplicationResponse response)
        {
            response.ContactPerson = new EServiceApplication.PersonTechnicalWithContactData
            {
                Id = intermediate.ContactPerson.Id,
                Persons = intermediate.ContactPerson.Persons.Select(n => new EServiceApplication.PersonTechnicalWithContactData.PersonData
                {
                    Id = n.Id,
                    ActiveFrom = n.ActiveFrom,
                    FirstName = n.FirstName,
                    LastName = n.LastName,
                    PrivatePersonalIdentifier = n.PrivatePersonalIdentifier
                }).ToArray(),
                ContactInformation = intermediate.ContactPerson.ContactInformation.Select(n => new EServiceApplication.PersonTechnicalWithContactData.ContactData
                {
                    Type = new EServiceApplication.ClassifierData
                    {
                        Code = n.Type.Code,
                        Id = n.Type.Id,
                        Value = n.Type.Value
                    },
                    Value = n.Value
                }).ToArray()
            };

            return response;
        }

        public static EServiceApplicationResponse.ApplicationResourceDataExpanded MapApplicationResource(EServiceApplicationIntermediateResponse.ApplicationResourceDataExpanded intermediate, EServiceApplicationResponse.ApplicationResourceDataExpanded response)
        {
            response.Id = intermediate.Id;
            response.AssignedResourceReturnDate = intermediate.AssignedResourceReturnDate;
            response.Attachments = intermediate.Attachments;
            response.EducationalInstitutionContactPersons = intermediate.EducationalInstitutionContactPersons;
            response.PNANumber = intermediate.PNANumber;
            response.PNAStatus = intermediate.PNAStatus;
            response.Resource = intermediate.Resource == null ? null : new EServiceApplicationResponse.ApplicationResourceDataExpanded.ResourceDataExpanded
            {
                Id = intermediate.Resource.Id,
                AcquisitionsValue = intermediate.Resource.AcquisitionsValue,
                InventoryNumber = intermediate.Resource.InventoryNumber,
                Manufacturer = intermediate.Resource.Manufacturer,
                ModelIdentifier = intermediate.Resource.ModelIdentifier,
                ModelName = intermediate.Resource.ModelName,
                Notes = intermediate.Resource.Notes,
                ResourceStatus = intermediate.Resource.ResourceStatus,
                ResourceSubType = new EServiceApplication.ClassifierData
                {
                    Id = intermediate.Resource.ResourceSubType.Id,
                    Code = intermediate.Resource.ResourceSubType.Code,
                    Value = intermediate.Resource.ResourceSubType.Value
                },
                SerialNumber = intermediate.Resource.SerialNumber
            };

            return response;
        }

        public static EServiceApplicationChangeContactPersonDto Map(EServiceContactPersonUpdateRequest model, EServiceApplicationChangeContactPersonDto dto)
        {
            dto.ContactInformationData = model.ApplicationContactInformation.Select(t => new EServiceApplicationChangeContactPersonDto.ContactInformation
            {
                TypeId = t.TypeId,
                Value = t.Value
            }).ToArray();

            return dto;
        }


        public static EServiceApplicationsChangeContactPersonDto Map(EServiceApplicationsContactPersonUpdateRequest model, EServiceApplicationsChangeContactPersonDto dto)
        {
            dto.ContactInformationData = model.ContactPerson.ApplicationContactInformation.Select(t => new EServiceApplicationsChangeContactPersonDto.ContactInformation
            {
                TypeId = t.TypeId,
                Value = t.Value
            }).ToArray();

            return dto;
        }

        public static EServiceChangeStatusDto Map(EServiceChangeStatusRequest model, EServiceChangeStatusDto dto)
        {
            dto.FileToDeleteIds = model.FileToDeleteIds;
            dto.Files = null;
            dto.Files = model.Files == null ? null : model.Files.Select(t =>
            {
                var bytes = Convert.FromBase64String(t.Content);

                using var stream = new System.IO.MemoryStream(bytes);

                var result = new FileDto
                {
                    FileName = t.FileName,
                    ContentType = t.ContentType,
                    Content = Utility.StreamToArray(stream),
                    SourceType = FileSourceType.S3
                };

                return result;
            });
            dto.Notes = model.Notes;

            return dto;
        }

        public static EServiceDocumentTemplatesDto Map(EServiceDocumentTemplatesRequest model, EServiceDocumentTemplatesDto dto)
        {
            dto.ResourceTypeId = model.ResourceTypeId;
            dto.EducationalInstitutionId = model.EducationalInstitutionId;

            return dto;
        }

        public static Expression<Func<DocumentTemplate, EServiceDocumentTemplateResponse>> ProjectDocumentTemplate()
        {
            return t => new EServiceDocumentTemplateResponse
            {
                Id = t.Id,
                Code = t.Code,
                Title = t.Title,
                Hyperlink = t.Hyperlink
            };
        }
    }
}
