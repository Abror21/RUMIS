using System;
using System.Collections.Generic;

namespace Izm.Rumis.Infrastructure.EServices.Dtos
{
    public class EServicesRelatedPersonResponseDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PrivatePersonalIdentifier { get; set; }
        public string BirthDate { get; set; }
        public IEnumerable<ActiveEducationDataResponse> ActiveEducationData { get; set; }

        public class ActiveEducationDataResponse
        {
            public string ClassGroup { get; set; }
            public string ClassGroupLevel { get; set; }
            public string EducationProgram { get; set; }
            public string EducationProgramCode { get; set; }
            public string EducationInstitutionName { get; set; }
            public int? EducationalInstitutionId { get; set; }
            public string EducationInstitutionCode { get; set; }
            public ClassifierData EducationInstitutionStatus { get; set; }
            public string SupervisorName { get; set; }
            public string SupervisorCode { get; set; }

            public class ClassifierData
            {
                public Guid Id { get; set; }
                public string Code { get; set; }
                public string Value { get; set; }
            }
        }
    }
}
