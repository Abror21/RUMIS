using Izm.Rumis.Domain.Enums;

namespace Izm.Rumis.Domain.Models
{
    public struct AccessLevel
    {
        public int? EducationalInstitutionId;
        public int? SupervisorId;
        public UserProfileType Type;
    }
}
