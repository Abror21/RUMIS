using Izm.Rumis.Domain.Entities;
using System;

namespace Izm.Rumis.Infrastructure.Logging
{
    public class Log
    {
        protected Log() { }

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Thread { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string TraceId { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string RequestUrl { get; set; }
        public string RequestMethod { get; set; }
        public string SessionId { get; set; }

        public Guid? UserId { get; set; }
        public virtual User User { get; set; }

        public Guid? UserProfileId { get; set; }
        public virtual UserProfile UserProfile { get; set; }

        public int? EducationalInstitutionId { get; set; }
        public virtual EducationalInstitution EducationalInstitution { get; set; }

        public int? SupervisorId { get; set; }
        public virtual Supervisor Supervisor { get; set; }

        public Guid? PersonId { get; set; }
        public virtual PersonTechnical Person { get; set; }
    }
}
