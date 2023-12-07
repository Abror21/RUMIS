using System;
using System.Collections.Generic;

namespace Izm.Rumis.Infrastructure.EServices.Dtos
{
    public class EServiceEmployeeResponseDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PrivatePersonalIdentifier { get; set; }
        public IEnumerable<ActiveWorkDataResponse> ActiveWorkData { get; set; }

        public class ActiveWorkDataResponse
        {
            public int EducationalInstitutionId { get; set; }
            public string EducationInstitutionCode { get; set; }
            public string EducationInstitutionName { get; set; }
            public string SupervisorName { get; set; }
            public string SupervisorCode { get; set; }
            public string PositionName { get; set; }
            public string PositionCode { get; set; }
        }
    }
}
