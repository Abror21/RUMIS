using Izm.Rumis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Api.Models
{
    public class ClassifierModel
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public string Payload { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsRequired { get; set; }
        public int? SortOrder { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public UserProfileType PermissionType { get; set; }
        public int? SupervisorId { get; set; }
        public int? EducationalInstitutionId { get; set; }
    }

    public class ClassifierEditModel
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [MaxLength(500)]
        public string Value { get; set; }

        [MaxLength(4000)]
        public string Payload { get; set; }
        public bool IsDisabled { get; set; }
        public int? SortOrder { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public int? SupervisorId { get; set; }
        public int? EducationalInstitutionId { get; set; }
    }

    public class ClassifierCreateModel : ClassifierEditModel
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public UserProfileType PermissionType { get; set; }
    }

    public class ClassifierTypeFilter
    {
        public IEnumerable<string> Types { get; set; }
        public bool IncludeDisabled { get; set; }
    }
}
