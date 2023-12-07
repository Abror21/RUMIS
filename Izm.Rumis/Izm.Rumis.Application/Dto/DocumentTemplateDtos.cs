using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using System;

namespace Izm.Rumis.Application.Dto
{
    public class DocumentTemplateEditDto : IAuthorizedDocumentTemplateEditDto
    {
        public string Title { get; set; }
        public string Code { get; set; }
        public DateOnly? ValidFrom { get; set; }
        public DateOnly? ValidTo { get; set; }
        /// <summary>
        /// Pass null to remove the file.
        /// </summary>
        public FileDto File { get; set; }
        public string Hyperlink { get; set; }
        public Guid ResourceTypeId { get; set; }
        public int? EducationalInstitutionId { get; set; }
        public int? SupervisorId { get; set; }
        public UserProfileType PermissionType { get; set; }
    }
}
