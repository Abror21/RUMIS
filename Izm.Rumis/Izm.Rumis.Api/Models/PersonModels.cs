using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Api.Models
{
    public class PersonCreateRequest
    {
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(12)]
        public string PrivatePersonalIdentifier { get; set; }
        public bool IsUser { get; set; }
        public DateTime? BirthDate { get; set; }
        public IEnumerable<ContactData> ContactInformation { get; set; } = Array.Empty<ContactData>();

        public class ContactData
        {
            public Guid TypeId { get; set; }

            [Required]
            [MaxLength(200)]
            public string Value { get; set; }
        }
    }

    public class PersonResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PrivatePersonalIdentifier { get; set; }
        public DateTime? BirthDate { get; set; }
        public Guid PersonTechnicalId { get; set; }
        public IEnumerable<ContactData> ContactInformation { get; set; } = Array.Empty<ContactData>();

        public class ContactData
        {
            public ClassifierData Type { get; set; }
            public string Value { get; set; }

            public class ClassifierData
            {
                public Guid Id { get; set; }
                public string Code { get; set; }
                public string Value { get; set; }
            }
        }
    }

    public class PersonCreateResponse
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
    }

    public class PersonUpdateRequest
    {
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(12)]
        public string PrivatePersonalIdentifier { get; set; }
        public DateTime? BirthDate { get; set; }
        public IEnumerable<ContactData> ContactInformation { get; set; } = Array.Empty<ContactData>();

        public class ContactData
        {
            public Guid TypeId { get; set; }

            [Required]
            [MaxLength(200)]
            public string Value { get; set; }
        }
    }
}
