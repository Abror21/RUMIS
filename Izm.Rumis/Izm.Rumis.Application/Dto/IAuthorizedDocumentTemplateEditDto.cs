using Izm.Rumis.Domain.Enums;

namespace Izm.Rumis.Domain.Entities
{
    public interface IAuthorizedDocumentTemplateEditDto
    {
        int? EducationalInstitutionId { get; }
        UserProfileType PermissionType { get; }
        int? SupervisorId { get; }
    }
}
