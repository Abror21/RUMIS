using Izm.Rumis.Domain.Events.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class User : Entity<Guid>
    {
        [MaxLength(30)]
        public string Name { get; set; }
        public bool IsTechnical { get; set; }
        public bool IsHidden { get; set; }

        public virtual PersonTechnical PersonTechnical { get; protected set; }
        public virtual ICollection<UserProfile> Profiles { get; protected set; } = new List<UserProfile>();

        public static User Create(string name = null, bool isTechnical = false, bool isHidden = false)
        {
            return Create(Guid.NewGuid());
        }

        public static User Create(Guid id, string name = null, bool isTechnical = false, bool isHidden = false)
        {
            var entity = new User
            {
                Id = id,
                Name = name,
                IsTechnical = isTechnical,
                IsHidden = isHidden
            };

            entity.Events.Add(new UserCreatedEvent(entity));

            return entity;
        }

        protected User() { }
    }
}
