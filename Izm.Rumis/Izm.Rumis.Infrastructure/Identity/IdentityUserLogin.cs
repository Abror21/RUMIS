using Izm.Rumis.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Infrastructure.Identity
{
    /// <summary>
    /// Identity user login
    /// </summary>
    public class IdentityUserLogin : Entity<Guid>
    {
        public IdentityUserLogin()
        {
            this.Id = Guid.NewGuid();
        }

        public UserAuthType AuthType { get; set; }

        /// <summary>
        /// User name used to authenticate a user.
        /// </summary>
        [MaxLength(100)]
        public string UserName { get; set; }

        public Guid UserId { get; set; }
        public virtual IdentityUser User { get; set; }

        #region local authentication (forms)

        public string PasswordHash { get; set; }

        [MaxLength(32)]
        public string PasswordResetKey { get; set; }

        public bool MustResetPassword { get; set; }

        #endregion
    }

    public enum UserAuthType
    {
        Forms,
        Adfs,
        Windows
    }
}
