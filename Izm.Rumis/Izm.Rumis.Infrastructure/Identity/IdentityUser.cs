using Izm.Rumis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Izm.Rumis.Infrastructure.Identity
{
    /// <summary>
    /// Identity user
    /// </summary>
    public class IdentityUser : Entity<Guid>
    {
        public bool IsDisabled { get; set; }

        public virtual ICollection<IdentityUserLogin> Logins { get; set; } = new List<IdentityUserLogin>();

        [ForeignKey("Id")]
        public virtual User User { get; set; }
    }
}
