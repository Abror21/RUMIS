using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Api.Models
{
    public class IdentityRoleModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ExternalName { get; set; }
        public bool ReadOnly { get; set; }
        public IEnumerable<string> Permissions { get; set; } = new List<string>();
    }

    public class IdentityRoleEditModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string ExternalName { get; set; }

        public IEnumerable<string> Permissions { get; set; } = new List<string>();
    }
}
