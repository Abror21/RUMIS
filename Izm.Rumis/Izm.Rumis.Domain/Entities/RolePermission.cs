using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class RolePermission : Entity<int>
    {
        [Required]
        [MaxLength(100)]
        public string Value { get; set; }

        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
    }
}
