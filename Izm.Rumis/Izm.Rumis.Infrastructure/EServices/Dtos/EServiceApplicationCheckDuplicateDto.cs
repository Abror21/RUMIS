using System;

namespace Izm.Rumis.Infrastructure.EServices.Dtos
{
    public class EServiceApplicationCheckDuplicateDto
    {
        public string PrivatePersonalIdentifier { get; set; }
        public Guid ResourceSubTypeId { get; set; }
    }
}
