using Izm.Rumis.Api.Common;
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
    public class DocumentTemplateIntermediateModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string Hyperlink { get; set; }
        public Classifier ResourceType { get; set; }
        public int? SupervisorId { get; set; }
        public UserProfileType PermissionType { get; set; }
        public Guid FileId { get; set; }
        public string FileName { get; set; }
    }

    public class DocumentTemplateModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public ClassifierData DocumentType { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string Hyperlink { get; set; }
        public ClassifierData ResourceType { get; set; }
        public int? SupervisorId { get; set; }
        public UserProfileType PermissionType { get; set; }

        public FileData File { get; set; }

        public class FileData
        {
            public Guid Id { get; set; }
            public string FileName { get; set; }
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
    }

    public class DocumentTemplateEditModel
    {
        [Required]
        [MaxLength(250)]
        public string Title { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [MaxLength(2000)]
        public string Hyperlink { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        [Required]
        public Guid ResourceTypeId { get; set; }

        public int? EducationalInstitutionId { get; set; }
        public int? SupervisorId { get; set; }
        public UserProfileType PermissionType { get; set; }

    }

    public class DocumentTemplateCreateModel : DocumentTemplateEditModel
    {
        public IFormFile File { get; set; } = null;
    }

    public class DocumentTemplateUpdateModel : DocumentTemplateEditModel
    {
        /// <summary>
        /// Pass null to leave current file as it is.
        /// </summary>
        public IFormFile File { get; set; } = null;
    }

    public class DocumentTemplateFilterRequest : Filter<DocumentTemplate>
    {
        public string Title { get; set; }
        public IEnumerable<string> Codes { get; set; }
        public string Hyperlink { get; set; }
        public DateTime? ValidFromMin { get; set; }
        public DateTime? ValidFromMax { get; set; }
        public DateTime? ValidToMin { get; set; }
        public DateTime? ValidToMax { get; set; }
        public IEnumerable<Guid> ResourceTypeIds { get; set; }
        public IEnumerable<UserProfileType> PermissionTypes { get; set; }
        public IEnumerable<int> SupervisorIds { get; set; }

        protected override Expression<Func<DocumentTemplate, bool>>[] GetFilters()
        {
            var filters = new List<Expression<Func<DocumentTemplate, bool>>>();

            if (!string.IsNullOrEmpty(Title))
                filters.Add(t => t.Title.Contains(Title));

            if (!string.IsNullOrEmpty(Hyperlink))
                filters.Add(t => t.Hyperlink.Contains(Hyperlink));

            if (ValidFromMin != null || ValidFromMax != null)
            {
                filters.Add(t => t.ValidFrom != null);
                if (ValidFromMin != null)
                    filters.Add(t => t.ValidFrom.Value >= DateOnly.FromDateTime(ValidFromMin.Value));
                if (ValidFromMax != null)
                    filters.Add(t => t.ValidFrom.Value <= DateOnly.FromDateTime(ValidFromMax.Value));
            }

            if (ValidToMin != null || ValidToMax != null)
            {
                filters.Add(t => t.ValidTo != null);
                if (ValidToMin != null)
                    filters.Add(t => t.ValidTo.Value >= DateOnly.FromDateTime(ValidToMin.Value));
                if (ValidToMax != null)
                    filters.Add(t => t.ValidTo.Value <= DateOnly.FromDateTime(ValidToMax.Value));
            }

            if (PermissionTypes != null && PermissionTypes.Any())
                filters.Add(t => PermissionTypes.Contains(t.PermissionType));

            if (SupervisorIds != null && SupervisorIds.Any())
                filters.Add(t => t.SupervisorId != null && SupervisorIds.Contains(t.SupervisorId.Value) ||
                 (PermissionTypes != null && PermissionTypes.Contains(UserProfileType.Country) && t.SupervisorId == null)); ;

            if (ResourceTypeIds != null && ResourceTypeIds.Any())
                filters.Add(t => ResourceTypeIds.Contains(t.ResourceTypeId));

            if (Codes != null && Codes.Any())
                filters.Add(t => Codes.Contains(t.Code));

            return filters.ToArray();
        }
    }
}
