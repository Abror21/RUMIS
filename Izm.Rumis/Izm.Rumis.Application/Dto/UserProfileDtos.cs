using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Izm.Rumis.Application.Dto
{
    public class UserProfileEditDto : IAuthorizedResourceCreateDto, IAuthorizedResourceEditDto
    {
        public Guid UserId { get; set; }
        public int? EducationalInstitutionId { get; set; }
        public string Email { get; set; }
        public DateTime Expires { get; set; }
        public bool IsDisabled { get; set; }
        public string Job { get; set; }
        public int? SupervisorId { get; set; }
        public string PhoneNumber { get; set; }
        public UserProfileType PermissionType { get; set; }
        public IEnumerable<int> RoleIds { get; set; } = Array.Empty<int>();
        public string ProfileCreationDocumentNumber { get; set; }
        public DateTime? ProfileCreationDocumentDate { get; set; }
        public string Notes { get; set; }
        public string ConfigurationInfo { get; set; }
        public Guid? InstitutionId { get; set; }
    }
}
