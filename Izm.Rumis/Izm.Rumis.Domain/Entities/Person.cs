using System;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class Person : Entity<Guid>
    {
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(11)]
        public string PrivatePersonalIdentifier { get; set; }
        public DateTime ActiveFrom { get; set; }
        public DateTime? BirthDate { get; set; }

        public Guid PersonTechnicalId { get; set; }
        public virtual PersonTechnical PersonTechnical { get; set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName} ({PrivatePersonalIdentifier})";
        }
    }
}
