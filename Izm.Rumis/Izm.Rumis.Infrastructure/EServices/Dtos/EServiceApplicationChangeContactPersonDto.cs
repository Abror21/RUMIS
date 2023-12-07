using System;
using System.Collections.Generic;

namespace Izm.Rumis.Infrastructure.EServices.Dtos
{
    public class EServiceApplicationChangeContactPersonDto
    {
        public IEnumerable<ContactInformation> ContactInformationData { get; set; } = Array.Empty<ContactInformation>();

        public class ContactInformation
        {
            public Guid TypeId { get; set; }
            public string Value { get; set; }
        }
    }
}
