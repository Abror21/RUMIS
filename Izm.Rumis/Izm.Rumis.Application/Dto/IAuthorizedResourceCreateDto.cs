using Izm.Rumis.Domain.Enums;

namespace Izm.Rumis.Domain.Entities
{
    public interface IAuthorizedResourceCreateDto
    {
        int? EducationalInstitutionId { get; }
        UserProfileType PermissionType { get; }
        int? SupervisorId { get; }
    }
}
