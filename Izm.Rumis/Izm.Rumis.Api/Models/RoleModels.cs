using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Api.Models
{
    public class RoleEditRequest
    {
        [Required]
        [MaxLength(10)]
        public string Code { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public IEnumerable<string> Permissions { get; set; } = new List<string>();
    }

    public class RoleResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Permissions { get; set; }
    }
}
