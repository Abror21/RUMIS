using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Application.Dto
{
    public class PersonCreateDto
    {
        public string FirstName { get; set; }
        public bool IsUser { get; set; }
        public string LastName { get; set; }
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

    public class PersonCreateResponseDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
    }

    public class PersonUpdateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
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
