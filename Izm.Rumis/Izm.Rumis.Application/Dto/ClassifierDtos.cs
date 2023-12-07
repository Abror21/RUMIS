using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Entities;
using System;

namespace Izm.Rumis.Application.Dto
{
    public abstract class ClassifierEditDto
    {
        public string Code { get; set; }
        public string Value { get; set; }
        public string Payload { get; set; }
        public bool IsDisabled { get; set; }
        public int? SortOrder { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public int? SupervisorId { get; set; }
        public int? EducationalInstitutionId { get; set; }
    }

    public class ClassifierCreateDto : ClassifierEditDto, IAuthorizedResourceCreateDto
    {
        public string Type { get; set; }
        public UserProfileType PermissionType { get; set; }
    }

    public class ClassifierUpdateDto : ClassifierEditDto, IAuthorizedResourceEditDto { }
}
