using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    public class ApplicationResourceMapper
    {
        public static Expression<Func<ApplicationResource, ApplicationResourceIntermediateListItem>> ProjectIntermediateListItem()
        {
            return t => new ApplicationResourceIntermediateListItem
            {
                Id = t.Id,
                Application = new ApplicationResourceIntermediateListItem.ApplicationDataExpanded
                {
                    Id = t.Application.Id,
                    ApplicationNumber = t.Application.ApplicationNumber,
                    EducationalInstitution = new ApplicationResourceListItem.ApplicationData.EducationalInstitutionData
                    {
                        Id = t.Application.EducationalInstitutionId,
                        Code = t.Application.EducationalInstitution.Code,
                        Name = t.Application.EducationalInstitution.Name
                    },
                    MonitoringClassGrade = t.Application.MonitoringClassGrade,
                    MonitoringClassParallel = t.Application.MonitoringClassParallel,
                    MonitoringEducationalStatus = t.Application.MonitoringEducationalStatus == null ? null : new ApplicationResourceListItem.ClassifierData
                    {
                        Id = t.Application.MonitoringEducationalStatusId.Value,
                        Code = t.Application.MonitoringEducationalStatus.Code,
                        Value = t.Application.MonitoringEducationalStatus.Value
                    },
                    MonitoringEducationalSubStatus = t.Application.MonitoringEducationalSubStatus == null ? null : new ApplicationResourceListItem.ClassifierData
                    {
                        Id = t.Application.MonitoringEducationalSubStatusId.Value,
                        Code = t.Application.MonitoringEducationalSubStatus.Code,
                        Value = t.Application.MonitoringEducationalSubStatus.Value
                    },
                    MonitoringGroup = t.Application.MonitoringGroup,
                    MonitoringWorkStatus = t.Application.MonitoringWorkStatus == null ? null : new ApplicationResourceListItem.ClassifierData
                    {
                        Id = t.Application.MonitoringWorkStatusId.Value,
                        Code = t.Application.MonitoringWorkStatus.Code,
                        Value = t.Application.MonitoringWorkStatus.Value
                    },
                    ResourceSubType = new ApplicationResourceListItem.ClassifierDataWithPayload
                    {
                        Id = t.Application.ResourceSubTypeId,
                        Code = t.Application.ResourceSubType.Code,
                        Value = t.Application.ResourceSubType.Value,
                        Payload = t.Application.ResourceSubType.Payload
                    },
                    ResourceTargetPerson = new ApplicationResourceListItem.ApplicationData.PersonTechnicalData
                    {
                        Id = t.Application.ResourceTargetPersonId,
                        Persons = t.Application.ResourceTargetPerson.Persons
                            .OrderByDescending(p => p.Created)
                            .Select(p => new ApplicationResourceListItem.ApplicationData.PersonTechnicalData.PersonData
                            {
                                Id = p.Id,
                                ActiveFrom = p.ActiveFrom,
                                FirstName = p.FirstName,
                                LastName = p.LastName,
                                PrivatePersonalIdentifier = p.PrivatePersonalIdentifier
                            })
                    },
                    ResourceTargetPersonClassGrade = t.Application.ResourceTargetPersonClassGrade,
                    ResourceTargetPersonClassParallel = t.Application.ResourceTargetPersonClassParallel,
                    ResourceTargetPersonEducationalStatus = t.Application.ResourceTargetPersonEducationalStatus == null ? null :
                        new ApplicationResourceListItem.ClassifierData
                        {
                            Id = t.Application.ResourceTargetPersonEducationalStatusId.Value,
                            Code = t.Application.ResourceTargetPersonEducationalStatus.Code,
                            Value = t.Application.ResourceTargetPersonEducationalStatus.Value
                        },
                    ResourceTargetPersonGroup = t.Application.ResourceTargetPersonGroup,
                    ResourceTargetPersonType = new ApplicationResourceListItem.ClassifierData
                    {
                        Id = t.Application.ResourceTargetPersonTypeId,
                        Code = t.Application.ResourceTargetPersonType.Code,
                        Value = t.Application.ResourceTargetPersonType.Value
                    },
                    ResourceTargetPersonWorkStatus = t.Application.ResourceTargetPersonWorkStatus == null ? null :
                        new ApplicationResourceListItem.ClassifierData
                        {
                            Id = t.Application.ResourceTargetPersonWorkStatusId.Value,
                            Code = t.Application.ResourceTargetPersonWorkStatus.Code,
                            Value = t.Application.ResourceTargetPersonWorkStatus.Value
                        },
                    Supervisor = new ApplicationResourceListItem.ApplicationData.SupervisorData
                    {
                        Id = t.Application.EducationalInstitution.SupervisorId,
                        Code = t.Application.EducationalInstitution.Supervisor.Code,
                        Name = t.Application.EducationalInstitution.Supervisor.Name
                    }
                },
                Attachments = t.ApplicationResourceAttachmentList
                    .Where(t => t.DocumentType.Code == DocumentType.PNA)
                    .OrderByDescending(t => t.DocumentDate)
                    .Select(a => new ApplicationResourceListItem.ApplicationResourceAttachmentData
                    {
                        DocumentDate = a.DocumentDate.ToDateTime(new TimeOnly())
                    }),
                AssignedResourceReturnDate = t.AssignedResourceReturnDate,
                CancelingDescription = t.CancelingDescription,
                CancelingReason = t.CancelingReason == null ? null :
                    new ApplicationResourceListItem.ClassifierData
                    {
                        Id = t.CancelingReason.Id,
                        Code = t.CancelingReason.Code,
                        Value = t.CancelingReason.Value
                    },
                PNANumber = t.PNANumber,
                PNAStatus = new ApplicationResourceListItem.ClassifierData
                {
                    Id = t.PNAStatusId,
                    Code = t.PNAStatus.Code,
                    Value = t.PNAStatus.Value
                },
                ReturnResourceDate = t.ReturnResourceDate,
                ReturnResourceState = t.ReturnResourceState == null ? null :
                    new ApplicationResourceListItem.ClassifierData
                    {
                        Id = t.ReturnResourceState.Id,
                        Code = t.ReturnResourceState.Code,
                        Value = t.ReturnResourceState.Value
                    },
                Resource = t.AssignedResource == null ? null :
                    new ApplicationResourceIntermediateListItem.ResourceDataExpanded
                    {
                        Id = t.AssignedResource.Id,
                        InventoryNumber = t.AssignedResource.InventoryNumber,
                        Manufacturer = new ApplicationResourceListItem.ClassifierData
                        {
                            Id = t.AssignedResource.ManufacturerId,
                            Code = t.AssignedResource.Manufacturer.Code,
                            Value = t.AssignedResource.Manufacturer.Value
                        },
                        ModelName = new ApplicationResourceListItem.ClassifierData
                        {
                            Id = t.AssignedResource.ModelNameId,
                            Code = t.AssignedResource.ModelName.Code,
                            Value = t.AssignedResource.ModelName.Value
                        },
                        ResourceNumber = t.AssignedResource.ResourceNumber,
                        ResourceSubType = new ApplicationResourceListItem.ClassifierDataWithPayload
                        {
                            Id = t.AssignedResource.ResourceSubTypeId,
                            Code = t.AssignedResource.ResourceSubType.Code,
                            Value = t.AssignedResource.ResourceSubType.Value,
                            Payload = t.AssignedResource.ResourceSubType.Payload
                        },
                        SerialNumber = t.AssignedResource.SerialNumber
                    },
                ResourceDiffer = t.AssignedResource == null ? false :
                    t.Application.ResourceSubTypeId != t.AssignedResource.ResourceSubTypeId
            };
        }

        public static ApplicationResourceListItemResponse Map(ApplicationResourceIntermediateListItem intermediate, ApplicationResourceListItemResponse response)
        {
            response.Id = intermediate.Id;
            response.Application = new ApplicationResourceListItemResponse.ApplicationDataExpanded
            {
                Id = intermediate.Application.Id,
                ApplicationNumber = intermediate.Application.ApplicationNumber,
                EducationalInstitution = intermediate.Application.EducationalInstitution,
                MonitoringClassGrade = intermediate.Application.MonitoringClassGrade,
                MonitoringClassParallel = intermediate.Application.MonitoringClassParallel,
                MonitoringEducationalStatus = intermediate.Application.MonitoringEducationalStatus,
                MonitoringEducationalSubStatus = intermediate.Application.MonitoringEducationalSubStatus,
                MonitoringGroup = intermediate.Application.MonitoringGroup,
                MonitoringWorkStatus = intermediate.Application.MonitoringWorkStatus,
                ResourceSubType = new ApplicationResourceListItem.ClassifierData
                {
                    Id = intermediate.Application.ResourceSubType.Id,
                    Code = intermediate.Application.ResourceSubType.Code,
                    Value = intermediate.Application.ResourceSubType.Value
                },
                ResourceTargetPerson = intermediate.Application.ResourceTargetPerson,
                ResourceTargetPersonClassGrade = intermediate.Application.ResourceTargetPersonClassGrade,
                ResourceTargetPersonClassParallel = intermediate.Application.ResourceTargetPersonClassParallel,
                ResourceTargetPersonEducationalStatus = intermediate.Application.ResourceTargetPersonEducationalStatus,
                ResourceTargetPersonGroup = intermediate.Application.ResourceTargetPersonGroup,
                ResourceTargetPersonType = intermediate.Application.ResourceTargetPersonType,
                ResourceTargetPersonWorkStatus = intermediate.Application.ResourceTargetPersonWorkStatus,
                Supervisor = intermediate.Application.Supervisor
            };
            response.Attachment = intermediate.Attachments.FirstOrDefault();
            response.AssignedResourceReturnDate = intermediate.AssignedResourceReturnDate;
            response.CancelingDescription = intermediate.CancelingDescription;
            response.CancelingReason = intermediate.CancelingReason;
            response.PNANumber = intermediate.PNANumber;
            response.PNAStatus = intermediate.PNAStatus;
            response.ReturnResourceDate = intermediate.ReturnResourceDate;
            response.ReturnResourceState = intermediate.ReturnResourceState;
            response.Resource = intermediate.Resource == null ? null :
                new ApplicationResourceListItemResponse.ResourceDataExpanded
                {
                    Id = intermediate.Resource.Id,
                    InventoryNumber = intermediate.Resource.InventoryNumber,
                    Manufacturer = intermediate.Resource.Manufacturer,
                    ModelName = intermediate.Resource.ModelName,
                    ResourceNumber = intermediate.Resource.ResourceNumber,
                    ResourceSubType = new ApplicationResourceListItem.ClassifierData
                    {
                        Id = intermediate.Resource.ResourceSubType.Id,
                        Code = intermediate.Resource.ResourceSubType.Code,
                        Value = intermediate.Resource.ResourceSubType.Value
                    },
                    SerialNumber = intermediate.Resource.SerialNumber
                };
            response.ResourceDiffer = intermediate.ResourceDiffer;

            return response;
        }

        public static Expression<Func<ApplicationResource, ApplicationResourceResponse>> Project()
        {
            return t => new ApplicationResourceResponse
            {
                Id = t.Id,
                Application = new ApplicationResourceResponse.ApplicationData
                {
                    Id = t.Application.Id,
                    ApplicationNumber = t.Application.ApplicationNumber,
                    EducationalInstitution = new ApplicationResourceResponse.ApplicationData.EducationalInstitutionData
                    {
                        Id = t.Application.EducationalInstitution.Id,
                        Code = t.Application.EducationalInstitution.Code,
                        Name = t.Application.EducationalInstitution.Name
                    },
                    MonitoringClassGrade = t.Application.MonitoringClassGrade,
                    MonitoringClassParallel = t.Application.MonitoringClassParallel,
                    MonitoringEducationalStatus = t.Application.MonitoringEducationalStatus == null ? null : new ApplicationResourceResponse.ClassifierData
                    {
                        Id = t.Application.MonitoringEducationalStatus.Id,
                        Code = t.Application.MonitoringEducationalStatus.Code,
                        Value = t.Application.MonitoringEducationalStatus.Value
                    },
                    MonitoringEducationalSubStatus = t.Application.MonitoringEducationalSubStatus == null ? null : new ApplicationResourceResponse.ClassifierData
                    {
                        Id = t.Application.MonitoringEducationalSubStatus.Id,
                        Code = t.Application.MonitoringEducationalSubStatus.Code,
                        Value = t.Application.MonitoringEducationalSubStatus.Value
                    },
                    MonitoringGroup = t.Application.MonitoringGroup,
                    MonitoringWorkStatus = t.Application.MonitoringWorkStatus == null ? null : new ApplicationResourceResponse.ClassifierData
                    {
                        Id = t.Application.MonitoringWorkStatus.Id,
                        Code = t.Application.MonitoringWorkStatus.Code,
                        Value = t.Application.MonitoringWorkStatus.Value
                    },
                    ResourceSubType = new ApplicationResourceResponse.ClassifierData
                    {
                        Id = t.Application.ResourceSubType.Id,
                        Code = t.Application.ResourceSubType.Code,
                        Value = t.Application.ResourceSubType.Value
                    },
                    ResourceTargetPerson = new ApplicationResourceResponse.ApplicationData.PersonTechnicalData
                    {
                        Id = t.Application.ResourceTargetPerson.Id,
                        Persons = t.Application.ResourceTargetPerson.Persons
                            .OrderByDescending(p => p.Created)
                            .Select(p => new ApplicationResourceResponse.ApplicationData.PersonTechnicalData.PersonData
                            {
                                Id = p.Id,
                                ActiveFrom = p.ActiveFrom,
                                FirstName = p.FirstName,
                                LastName = p.LastName,
                                PrivatePersonalIdentifier = p.PrivatePersonalIdentifier
                            })
                    },
                    ResourceTargetPersonClassGrade = t.Application.ResourceTargetPersonClassGrade,
                    ResourceTargetPersonClassParallel = t.Application.ResourceTargetPersonClassParallel,
                    ResourceTargetPersonEducationalStatus = t.Application.ResourceTargetPersonEducationalStatus == null ? null :
                        new ApplicationResourceResponse.ClassifierData
                        {
                            Id = t.Application.ResourceTargetPersonEducationalStatus.Id,
                            Code = t.Application.ResourceTargetPersonEducationalStatus.Code,
                            Value = t.Application.ResourceTargetPersonEducationalStatus.Value
                        },
                    ResourceTargetPersonGroup = t.Application.ResourceTargetPersonGroup,
                    ResourceTargetPersonType = new ApplicationResourceResponse.ClassifierData
                    {
                        Id = t.Application.ResourceTargetPersonType.Id,
                        Code = t.Application.ResourceTargetPersonType.Code,
                        Value = t.Application.ResourceTargetPersonType.Value
                    },
                    ResourceTargetPersonWorkStatus = t.Application.ResourceTargetPersonWorkStatus == null ? null :
                        new ApplicationResourceResponse.ClassifierData
                        {
                            Id = t.Application.ResourceTargetPersonWorkStatus.Id,
                            Code = t.Application.ResourceTargetPersonWorkStatus.Code,
                            Value = t.Application.ResourceTargetPersonWorkStatus.Value
                        },
                    Supervisor = new ApplicationResourceResponse.ApplicationData.SupervisorData
                    {
                        Id = t.Application.EducationalInstitution.Supervisor.Id,
                        Code = t.Application.EducationalInstitution.Supervisor.Code,
                        Name = t.Application.EducationalInstitution.Supervisor.Name
                    }
                },
                AssignedResource = t.AssignedResource == null ? null :
                    new ApplicationResourceResponse.ResourceData
                    {
                        Id = t.AssignedResource.Id,
                        InventoryNumber = t.AssignedResource.InventoryNumber,
                        Manufacturer = new ApplicationResourceResponse.ClassifierData
                        {
                            Id = t.AssignedResource.Manufacturer.Id,
                            Code = t.AssignedResource.Manufacturer.Code,
                            Value = t.AssignedResource.Manufacturer.Value
                        },
                        ModelName = new ApplicationResourceResponse.ClassifierData
                        {
                            Id = t.AssignedResource.ModelName.Id,
                            Code = t.AssignedResource.ModelName.Code,
                            Value = t.AssignedResource.ModelName.Value
                        },
                        ResourceNumber = t.AssignedResource.ResourceNumber,
                        ResourceSubType = new ApplicationResourceResponse.ClassifierDataWithPayload
                        {
                            Id = t.AssignedResource.ResourceSubType.Id,
                            Code = t.AssignedResource.ResourceSubType.Code,
                            Value = t.AssignedResource.ResourceSubType.Value,
                            Payload = t.AssignedResource.ResourceSubType.Payload
                        },
                        SerialNumber = t.AssignedResource.SerialNumber
                    },
                AssignedResourceDiffer = t.Application.ResourceSubTypeId != t.AssignedResource.ResourceSubTypeId,
                AssignedResourceReturnDate = t.AssignedResourceReturnDate,
                AssignedResourceState = t.AssignedResourceState == null ? null :
                    new ApplicationResourceResponse.ClassifierData
                    {
                        Id = t.AssignedResourceState.Id,
                        Code = t.AssignedResourceState.Code,
                        Value = t.AssignedResourceState.Value
                    },
                Attachments = t.ApplicationResourceAttachmentList.Select(a => new ApplicationResourceResponse.ApplicationResourceAttachmentData
                {
                    Id = a.Id,
                    DocumentDate = a.DocumentDate.ToDateTime(new TimeOnly()),
                    DocumentType = a.DocumentType == null ? null :
                        new ApplicationResourceResponse.ClassifierData
                        {
                            Id = a.DocumentType.Id,
                            Code = a.DocumentType.Code,
                            Value = a.DocumentType.Value
                        },
                    DocumentTemplate = a.DocumentTemplate == null ? null :
                        new ApplicationResourceResponse.ApplicationResourceAttachmentData.DocumentTemplateData
                        {
                            Id = a.DocumentTemplate.Id,
                            Code = a.DocumentTemplate.Code,
                            Title = a.DocumentTemplate.Title,
                            ValidFrom = a.DocumentTemplate.ValidFrom,
                            ValidTo = a.DocumentTemplate.ValidTo,
                            FileName = a.DocumentTemplate.FileName
                        },
                    File = a.File == null ? null :
                        new ApplicationResourceResponse.ApplicationResourceAttachmentData.FileData
                        {
                            Id = a.File.Id,
                            Name = a.File.Name,
                            Extension = a.File.Extension,
                            ContentType = a.File.ContentType,
                            SourceType = a.File.SourceType,
                            Length = a.File.Length,
                            Content = a.File.Content,
                            BucketName = a.File.BucketName
                        }
                }),
                EducationalInstitutionContactPersons = t.ApplicationResourceContactPersons.Select(c => new ApplicationResourceResponse.EducationalInstitutionContactPersonData
                {
                    Id = c.EducationalInstitutionContactPerson.Id,
                    Name = c.EducationalInstitutionContactPerson.Name,
                    Email = c.EducationalInstitutionContactPerson.Email,
                    PhoneNumber = c.EducationalInstitutionContactPerson.PhoneNumber,
                    JobPosition = new ApplicationResourceResponse.ClassifierData
                    {
                        Id = c.EducationalInstitutionContactPerson.JobPosition.Id,
                        Code = c.EducationalInstitutionContactPerson.JobPosition.Code,
                        Value = c.EducationalInstitutionContactPerson.JobPosition.Value
                    },
                }),
                Notes = t.Notes,
                PNANumber = t.PNANumber,
                PNAStatus = new ApplicationResourceResponse.ClassifierData
                {
                    Id = t.PNAStatus.Id,
                    Code = t.PNAStatus.Code,
                    Value = t.PNAStatus.Value
                },
                ReturnResourceDate = t.ReturnResourceDate,
                ReturnResourceState = t.ReturnResourceState == null ? null :
                    new ApplicationResourceResponse.ClassifierData
                    {
                        Id = t.ReturnResourceState.Id,
                        Code = t.ReturnResourceState.Code,
                        Value = t.ReturnResourceState.Value
                    },
                CancelingDescription = t.CancelingDescription,
                CancelingReason = t.CancelingReason == null ? null :
                    new ApplicationResourceResponse.ClassifierData
                    {
                        Id = t.CancelingReason.Id,
                        Code = t.CancelingReason.Code,
                        Value = t.CancelingReason.Value
                    }
            };
        }

        public static Expression<Func<ApplicationResourceAttachment, ApplicationResourceAttachmentData>> ProjectAttachment()
        {
            return t => new ApplicationResourceAttachmentData
            {
                ApplicationResourceId = t.ApplicationResourceId,
                DocumentDate = t.DocumentDate.ToDateTime(new TimeOnly())
            };
        }

        public static ApplicationResourceEditDto Map(ApplicationResourceEditRequest model, ApplicationResourceEditDto dto)
        {
            dto.AssignedResourceId = model.AssignedResourceId;
            dto.AssignedResourceReturnDate = model.AssignedResourceReturnDate;
            dto.EducationalInstitutionContactPersonIds = model.EducationalInstitutionContactPersonIds;
            dto.Notes = model.Notes;

            return dto;
        }

        public static ApplicationResourceCreateDto Map(ApplicationResourceCreateRequest model, ApplicationResourceCreateDto dto)
        {
            dto.ApplicationId = model.ApplicationId;
            Map(model, (ApplicationResourceEditDto)dto);

            return dto;
        }

        public static ApplicationResourceCancelDto Map(ApplicationResourceCancelRequest model, ApplicationResourceCancelDto dto)
        {
            dto.ReasonId = model.ReasonId;
            dto.Description = model.Description;
            dto.ChangeApplicationStatusToWithdrawn = model.ChangeApplicationStatusToWithdrawn;

            return dto;
        }

        public static ApplicationResourceReturnDeadlineDto Map(ApplicationResourceReturnDeadlineRequest model, ApplicationResourceReturnDeadlineDto dto)
        {
            dto.ApplicationResourceIds = model.ApplicationResourceIds;
            dto.AssignedResourceReturnDate = model.AssignedResourceReturnDate;

            return dto;
        }

        public static ApplicationResourceChangeStatusDto Map(ApplicationResourceChangeStatusRequest model, ApplicationResourceChangeStatusDto dto)
        {
            dto.FileToDeleteIds = model.FileToDeleteIds;
            dto.Files = model.Files == null ? null :
                model.Files.Select(t => Mapper.MapFile(t, new FileDto { SourceType = FileSourceType.S3 }));
            dto.Notes = model.Notes;

            return dto;
        }

        public static ApplicationResourceReturnEditDto Map(ApplicationResourceReturnEditRequest model, ApplicationResourceReturnEditDto dto)
        {
            dto.ResourceStatusId = model.ResourceStatusId;
            dto.ReturnResourceStateId = model.ReturnResourceStateId;
            dto.ReturnResourceDate = model.ReturnResourceDate;
            dto.Notes = model.Notes;

            return dto;
        }
    }
}
