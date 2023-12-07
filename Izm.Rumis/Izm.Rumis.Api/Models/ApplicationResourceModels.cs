using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Models
{
    public class ApplicationResourceFilterRequest : Filter<ApplicationResource>
    {
        [DataType(DataType.Date)]
        public DateTime? DocumentDateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DocumentDateTo { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public IEnumerable<int> EducationalInstitutionIds { get; set; }
        public string InventoryNumber { get; set; }
        public IEnumerable<Guid> PNAStatusIds { get; set; }
        public DateTime? ReturnDateFrom { get; set; }
        public DateTime? ReturnDateTo { get; set; }
        public IEnumerable<Guid> ReturnResourceStateIds { get; set; }
        public bool? ResourceDiffer { get; set; }
        public string ResourceName { get; set; }
        public string ResourceNumber { get; set; }
        public IEnumerable<Guid> ResourceSubTypeIds { get; set; }
        public string ResourceTargetPerson { get; set; }
        public IEnumerable<Guid> ResourceTargetPersonTypeIds { get; set; }
        public IEnumerable<Guid> ResourceTargetPersonEducationalStatusIds { get; set; }
        public IEnumerable<Guid> ResourceTargetPersonWorkStatusIds { get; set; }
        public string SerialNumber { get; set; }
        public string SubmitterPerson { get; set; }
        public IEnumerable<int> SupervisorIds { get; set; }

        protected override Expression<Func<ApplicationResource, bool>>[] GetFilters()
        {
            var filters = new List<Expression<Func<ApplicationResource, bool>>>();

            if (DocumentDateFrom != null)
                filters.Add(t => t.ApplicationResourceAttachmentList
                                    .Where(a => a.DocumentType.Code == DocumentType.PNA)
                                    .OrderBy(a => a.DocumentDate)
                                    .Select(a => a.DocumentDate)
                                    .Last() >= Mapper.MapDateOnly(DocumentDateFrom));

            if (DocumentDateTo != null)
                filters.Add(t => t.ApplicationResourceAttachmentList
                                    .Where(a => a.DocumentType.Code == DocumentType.PNA)
                                    .OrderBy(a => a.DocumentDate)
                                    .Select(a => a.DocumentDate)
                                    .Last() <= Mapper.MapDateOnly(DocumentDateTo));

            if (DueDateFrom != null)
                filters.Add(t => t.AssignedResourceReturnDate >= DueDateFrom);

            if (DueDateTo != null)
                filters.Add(t => t.AssignedResourceReturnDate <= DueDateTo);

            if (EducationalInstitutionIds != null && EducationalInstitutionIds.Any())
                filters.Add(t => EducationalInstitutionIds.Contains(t.Application.EducationalInstitutionId));

            if (!string.IsNullOrEmpty(InventoryNumber))
                filters.Add(t => t.AssignedResource.InventoryNumber.Contains(InventoryNumber));

            if (PNAStatusIds != null && PNAStatusIds.Any())
                filters.Add(t => PNAStatusIds.Contains(t.PNAStatusId));

            if (ReturnDateFrom != null)
                filters.Add(t => t.ReturnResourceDate >= ReturnDateFrom);

            if (ReturnDateTo != null)
                filters.Add(t => t.ReturnResourceDate <= ReturnDateTo);

            if (ReturnResourceStateIds != null && ReturnResourceStateIds.Any())
                filters.Add(t => ReturnResourceStateIds.Contains(t.ReturnResourceStateId.Value));

            if (ResourceDiffer != null)
                filters.Add(t => ResourceDiffer.Value == (t.Application.ResourceSubTypeId != t.AssignedResource.ResourceSubTypeId));

            if (!string.IsNullOrEmpty(ResourceName))
                filters.Add(t => t.AssignedResource.ResourceName.Contains(ResourceName));

            if (!string.IsNullOrEmpty(ResourceNumber))
                filters.Add(t => t.AssignedResource.ResourceNumber.Contains(ResourceNumber));

            if (ResourceSubTypeIds != null && ResourceSubTypeIds.Any())
                filters.Add(t => ResourceSubTypeIds.Contains(t.AssignedResource.ResourceSubTypeId));

            if (!string.IsNullOrEmpty(ResourceTargetPerson))
                filters.Add(t => t.Application.ResourceTargetPerson.Persons
                                    .Any(t => t.FirstName.Contains(ResourceTargetPerson)
                                                || t.LastName.Contains(ResourceTargetPerson)
                                                || t.PrivatePersonalIdentifier.Contains(ResourceTargetPerson)));

            if (ResourceTargetPersonTypeIds != null && ResourceTargetPersonTypeIds.Any())
                filters.Add(t => ResourceTargetPersonTypeIds.Contains(t.Application.ResourceTargetPersonTypeId));

            if (ResourceTargetPersonEducationalStatusIds != null && ResourceTargetPersonEducationalStatusIds.Any())
                filters.Add(t =>
                    t.Application.MonitoringEducationalStatusId.HasValue
                    ? ResourceTargetPersonEducationalStatusIds.Contains(t.Application.MonitoringEducationalStatusId.Value)
                    : ResourceTargetPersonEducationalStatusIds.Contains(t.Application.ResourceTargetPersonEducationalStatusId.Value));

            if (ResourceTargetPersonWorkStatusIds != null && ResourceTargetPersonWorkStatusIds.Any())
                filters.Add(t =>
                    t.Application.MonitoringWorkStatusId.HasValue
                    ? ResourceTargetPersonWorkStatusIds.Contains(t.Application.MonitoringWorkStatusId.Value)
                    : ResourceTargetPersonWorkStatusIds.Contains(t.Application.ResourceTargetPersonWorkStatusId.Value));

            if (!string.IsNullOrEmpty(SerialNumber))
                filters.Add(t => t.AssignedResource.SerialNumber.Contains(SerialNumber));

            if (!string.IsNullOrEmpty(SubmitterPerson))
                filters.Add(t => t.Application.SubmitterPerson.Persons
                                    .Any(t => t.FirstName.Contains(SubmitterPerson)
                                                || t.LastName.Contains(SubmitterPerson)
                                                || t.PrivatePersonalIdentifier.Contains(SubmitterPerson)));

            if (SupervisorIds != null && SupervisorIds.Any())
                filters.Add(t => SupervisorIds.Contains(t.Application.EducationalInstitution.SupervisorId));

            return filters.ToArray();
        }
    }

    public abstract class ApplicationResourceListItem
    {
        public Guid Id { get; set; }
        public DateTime? AssignedResourceReturnDate { get; set; }
        public string CancelingDescription { get; set; }
        public ClassifierData CancelingReason { get; set; }
        public string PNANumber { get; set; }
        public ClassifierData PNAStatus { get; set; }
        public DateTime? ReturnResourceDate { get; set; }
        public ClassifierData ReturnResourceState { get; set; }
        public bool ResourceDiffer { get; set; }

        public class ApplicationResourceAttachmentData
        {
            [DataType(DataType.Date)]
            public DateTime DocumentDate { get; set; }
        }

        public class ApplicationData
        {
            public Guid Id { get; set; }
            public string ApplicationNumber { get; set; }
            public EducationalInstitutionData EducationalInstitution { get; set; }
            public int? MonitoringClassGrade { get; set; }
            public string MonitoringClassParallel { get; set; }
            public ClassifierData MonitoringEducationalStatus { get; set; }
            public ClassifierData MonitoringEducationalSubStatus { get; set; }
            public string MonitoringGroup { get; set; }
            public ClassifierData MonitoringWorkStatus { get; set; }
            public PersonTechnicalData ResourceTargetPerson { get; set; }
            public int? ResourceTargetPersonClassGrade { get; set; }
            public string ResourceTargetPersonClassParallel { get; set; }
            public ClassifierData ResourceTargetPersonEducationalStatus { get; set; }
            public string ResourceTargetPersonGroup { get; set; }
            public ClassifierData ResourceTargetPersonType { get; set; }
            public ClassifierData ResourceTargetPersonWorkStatus { get; set; }
            public SupervisorData Supervisor { get; set; }

            public class PersonTechnicalData
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

            public class EducationalInstitutionData
            {
                public int Id { get; set; }
                public string Code { get; set; }
                public string Name { get; set; }
            }

            public class SupervisorData
            {
                public int Id { get; set; }
                public string Code { get; set; }
                public string Name { get; set; }
            }
        }

        public class ResourceData
        {
            public Guid Id { get; set; }
            public string InventoryNumber { get; set; }
            public ClassifierData Manufacturer { get; set; }
            public ClassifierData ModelName { get; set; }
            public string ResourceNumber { get; set; }
            public string SerialNumber { get; set; }
        }

        public class ClassifierData
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Value { get; set; }

            public static Expression<Func<Classifier, ClassifierData>> Project()
            {
                return t => new ClassifierData
                {
                    Id = t.Id,
                    Code = t.Code,
                    Value = t.Value
                };
            }
        }

        public class ClassifierDataWithPayload : ClassifierData
        {
            public string Payload { get; set; }
        }
    }

    public class ApplicationResourceListItemResponse : ApplicationResourceListItem
    {
        public ApplicationDataExpanded Application { get; set; }
        public ApplicationResourceAttachmentData Attachment { get; set; }
        public ResourceDataExpanded Resource { get; set; }

        public class ApplicationDataExpanded : ApplicationData
        {
            public ClassifierData ResourceType { get; set; }
            public ClassifierData ResourceSubType { get; set; }
        }

        public class ResourceDataExpanded : ResourceData
        {
            public ClassifierData ResourceType { get; set; }
            public ClassifierData ResourceSubType { get; set; }
        }
    }

    public class ApplicationResourceIntermediateListItem : ApplicationResourceListItem
    {
        public ApplicationDataExpanded Application { get; set; }
        public IEnumerable<ApplicationResourceAttachmentData> Attachments { get; set; }
        public ResourceDataExpanded Resource { get; set; }

        public class ApplicationDataExpanded : ApplicationData
        {
            public ClassifierDataWithPayload ResourceSubType { get; set; }
        }

        public class ResourceDataExpanded : ResourceData
        {
            public ClassifierDataWithPayload ResourceSubType { get; set; }
        }
    }

    public class ApplicationResourceResponse
    {
        public Guid Id { get; set; }
        public ApplicationData Application { get; set; }
        public ResourceData AssignedResource { get; set; }
        public bool AssignedResourceDiffer { get; set; }
        public DateTime? AssignedResourceReturnDate { get; set; }
        public ClassifierData AssignedResourceState { get; set; }
        public IEnumerable<ApplicationResourceAttachmentData> Attachments { get; set; }
        public IEnumerable<EducationalInstitutionContactPersonData> EducationalInstitutionContactPersons { get; set; }
        public string Notes { get; set; }
        public string PNANumber { get; set; }
        public ClassifierData PNAStatus { get; set; }
        public DateTime? ReturnResourceDate { get; set; }
        public ClassifierData ReturnResourceState { get; set; }
        public string CancelingDescription { get; set; }
        public ClassifierData CancelingReason { get; set; }

        public class ApplicationData
        {
            public Guid Id { get; set; }
            public string ApplicationNumber { get; set; }
            public EducationalInstitutionData EducationalInstitution { get; set; }
            public int? MonitoringClassGrade { get; set; }
            public string MonitoringClassParallel { get; set; }
            public ClassifierData MonitoringEducationalStatus { get; set; }
            public ClassifierData MonitoringEducationalSubStatus { get; set; }
            public string MonitoringGroup { get; set; }
            public ClassifierData MonitoringWorkStatus { get; set; }
            public ClassifierData ResourceSubType { get; set; }
            public PersonTechnicalData ResourceTargetPerson { get; set; }
            public int? ResourceTargetPersonClassGrade { get; set; }
            public string ResourceTargetPersonClassParallel { get; set; }
            public ClassifierData ResourceTargetPersonEducationalStatus { get; set; }
            public string ResourceTargetPersonGroup { get; set; }
            public ClassifierData ResourceTargetPersonType { get; set; }
            public ClassifierData ResourceTargetPersonWorkStatus { get; set; }
            public SupervisorData Supervisor { get; set; }

            public class PersonTechnicalData
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

            public class EducationalInstitutionData
            {
                public int Id { get; set; }
                public string Code { get; set; }
                public string Name { get; set; }
            }

            public class SupervisorData
            {
                public int Id { get; set; }
                public string Code { get; set; }
                public string Name { get; set; }
            }
        }

        public class ApplicationResourceAttachmentData
        {
            public Guid Id { get; set; }
            public DateTime DocumentDate { get; set; }
            public ClassifierData DocumentType { get; set; }
            public DocumentTemplateData DocumentTemplate { get; set; }
            public FileData File { get; set; }

            public class DocumentTemplateData
            {
                public int Id { get; set; }
                public string Code { get; set; }
                public string Title { get; set; }
                public DateOnly? ValidFrom { get; set; }
                public DateOnly? ValidTo { get; set; }
                public string FileName { get; set; }
            }

            public class FileData
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
                public string Extension { get; set; }
                public string ContentType { get; set; }
                public FileSourceType SourceType { get; set; }
                public int Length { get; set; }
                public byte[] Content { get; set; }
                public string BucketName { get; set; }
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

        public class ResourceData
        {
            public Guid Id { get; set; }
            public string InventoryNumber { get; set; }
            public ClassifierData Manufacturer { get; set; }
            public ClassifierData ModelName { get; set; }
            public string ResourceNumber { get; set; }
            public ClassifierData ResourceType { get; set; }
            public ClassifierDataWithPayload ResourceSubType { get; set; }
            public string SerialNumber { get; set; }
        }

        public class ClassifierData
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Value { get; set; }

            public static Expression<Func<Classifier, ClassifierData>> Project()
            {
                return t => new ClassifierData
                {
                    Id = t.Id,
                    Code = t.Code,
                    Value = t.Value
                };
            }
        }

        public class ClassifierDataWithPayload : ClassifierData
        {
            public string Payload { get; set; }
        }
    }

    public class ApplicationResourceAttachmentData : ApplicationResourceListItemResponse.ApplicationResourceAttachmentData
    {
        public Guid ApplicationResourceId { get; set; }
    }

    public class ApplicationResourceEditRequest
    {
        public Guid? AssignedResourceId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? AssignedResourceReturnDate { get; set; }
        public IEnumerable<Guid> EducationalInstitutionContactPersonIds { get; set; }
        public string Notes { get; set; }
    }

    public class ApplicationResourceCreateRequest : ApplicationResourceEditRequest
    {
        public Guid ApplicationId { get; set; }
    }

    public class ApplicationResourceUpdateRequest : ApplicationResourceEditRequest { }

    public class ApplicationResourceCancelRequest
    {
        public Guid ReasonId { get; set; }
        public string Description { get; set; }
        public bool ChangeApplicationStatusToWithdrawn { get; set; }
    }

    public class ApplicationResourceChangeStatusRequest
    {
        public IEnumerable<Guid> FileToDeleteIds { get; set; }
        public IEnumerable<IFormFile> Files { get; set; } = Array.Empty<IFormFile>();
        public string Notes { get; set; }
    }

    public class ApplicationResourceReturnEditRequest
    {
        public Guid ResourceStatusId { get; set; }
        public Guid ReturnResourceStateId { get; set; }
        public DateTime ReturnResourceDate { get; set; }
        public string Notes { get; set; }
    }

    public class ApplicationResourceReturnDeadlineRequest
    {
        public IEnumerable<Guid> ApplicationResourceIds { get; set; }
        public DateTime AssignedResourceReturnDate { get; set; }
    }
}
