using Izm.Rumis.Domain.Enums;

namespace Izm.Rumis.Domain.Entities
{
    public interface IAuthorizedResourceEditDto
    {
        int? EducationalInstitutionId { get; }
        int? SupervisorId { get; }
    }
}
