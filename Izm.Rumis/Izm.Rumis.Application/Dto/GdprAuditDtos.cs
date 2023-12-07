using Izm.Rumis.Domain.Models;
using System;
using System.Collections.Generic;

namespace Izm.Rumis.Application.Dto
{
    public class GdprAuditTraceDto
    {
        public string Action { get; set; }
        public string ActionData { get; set; }
        public Guid? DataOwnerId { get; set; }
        public string DataOwnerPrivatePersonalIdentifier { get; set; }
        public int? EducationalInstitutionId { get; set; }
        public IEnumerable<PersonDataProperty> Data { get; set; } = Array.Empty<PersonDataProperty>();
    }
}
