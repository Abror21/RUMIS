using System;
using System.Collections.Generic;
using System.Linq;

namespace Izm.Rumis.Domain.Entities
{
    public class PersonTechnical : Entity<Guid>
    {
        public Guid? UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<Person> Persons { get; protected set; } = new List<Person>();
        public virtual ICollection<PersonContact> PersonContacts { get; set; } = new List<PersonContact>();

        public override string ToString()
        {
            return Persons.OrderBy(t => t.Created).Last().ToString();
        }

        public Person GetPerson()
        {
            return Persons.OrderBy(t => t.Created).Last();
        }

        public void CreateUser()
        {
            User = User.Create();
        }
    }
}
