using System;

namespace Izm.Rumis.Infrastructure.EServices.Dtos
{
    public class EServicesEducationalInstitutionDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public ClassifierData Status { get; set; }
        public SupervisorData Supervisor { get; set; }

        public class ClassifierData
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Value { get; set; }
        }

        public class SupervisorData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }
}
