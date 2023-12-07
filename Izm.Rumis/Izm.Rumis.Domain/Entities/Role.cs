using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class Role : Entity<int>
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(10)]
        public string Code { get; set; }

        public virtual ICollection<UserProfile> UserProfiles { get; protected set; } = new List<UserProfile>();
        public virtual ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
    }
}
