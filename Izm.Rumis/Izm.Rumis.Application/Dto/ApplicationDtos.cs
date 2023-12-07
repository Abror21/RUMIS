using Izm.Rumis.Application.Models;
using System;
using System.Collections.Generic;

namespace Izm.Rumis.Application.Dto
{
    public class ApplicationCreateDto
    {
        public IEnumerable<Guid> ApplicationSocialStatuses { get; set; } = Array.Empty<Guid>();
        public string ApplicationStatusHistory { get; set; }
        public int EducationalInstitutionId { get; set; }
        public string Notes { get; set; }
        public Guid ResourceSubTypeId { get; set; }
        public int? ResourceTargetPersonClassGrade { get; set; }
        public string ResourceTargetPersonClassParallel { get; set; }
        public string ResourceTargetPersonEducationalProgram { get; set; }
        public Guid? ResourceTargetPersonEducationalStatusId { get; set; }
        public Guid? ResourceTargetPersonEducationalSubStatusId { get; set; }
        public string ResourceTargetPersonGroup { get; set; }
        public PersonData ResourceTargetPerson { get; set; }
        public Guid ResourceTargetPersonTypeId { get; set; }
        public Guid? ResourceTargetPersonWorkStatusId { get; set; }
        public bool? SocialStatus { get; set; }
        public bool? SocialStatusApproved { get; set; }
        public PersonData SubmitterPerson { get; set; }
        public Guid SubmitterTypeId { get; set; }

    }

    public class ApplicationUpdateDto
    {
        public string ApplicationStatusHistory { get; set; }
        public Guid ApplicationStatusId { get; set; }
        public Guid ContactPersonId { get; set; }
        public string Notes { get; set; }
        public Guid ResourceSubTypeId { get; set; }
        public int? ResourceTargetPersonClassGrade { get; set; }
        public string ResourceTargetPersonClassParallel { get; set; }
        public string ResourceTargetPersonEducationalProgram { get; set; }
        public Guid? ResourceTargetPersonEducationalStatusId { get; set; }
        public Guid? ResourceTargetPersonEducationalSubStatusId { get; set; }
        public string ResourceTargetPersonGroup { get; set; }
        public Guid ResourceTargetPersonId { get; set; }
        public Guid ResourceTargetPersonTypeId { get; set; }
        public Guid? ResourceTargetPersonWorkStatusId { get; set; }
        public bool? SocialStatus { get; set; }
        public bool? SocialStatusApproved { get; set; }
        public Guid SubmitterTypeId { get; set; }
    }

    public class ApplicationDeclineDto
    {
        public IEnumerable<Guid> ApplicationIds { get; set; }
        public string Reason { get; set; }
    }

    public class ApplicationContactInformationUpdateDto
    {
        public PersonData Person { get; set; }
    }

    public class ApplicationsContactInformationUpdateDto
    {
        public PersonData Person { get; set; }
    }

    public class ApplicationCheckDuplicateDto
    {
        public string PrivatePersonalIdentifier { get; set; }
        public Guid ResourceSubTypeId { get; set; }
    }
}
