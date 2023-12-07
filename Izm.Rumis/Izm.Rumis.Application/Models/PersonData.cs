using System;
using System.Collections.Generic;

namespace Izm.Rumis.Application.Models
{
    public class PersonData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PrivatePersonalIdentifier { get; set; }
        public IEnumerable<ContactData> ContactInformation { get; set; } = Array.Empty<ContactData>();

        public class ContactData
        {
            public Guid TypeId { get; set; }
            public string Value { get; set; }
        }
    }
}
