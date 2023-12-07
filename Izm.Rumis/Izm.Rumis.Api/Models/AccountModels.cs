using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Api.Models
{
    public class AccountPasswordResetModel
    {
        [Required]
        [MaxLength(100)]
        public string Secret { get; set; }

        [Required]
        [MaxLength(100)]
        public string Password { get; set; }
    }

    public class AccountPasswordChangeModel
    {
        [Required]
        [MaxLength(100)]
        public string CurrentPassword { get; set; }

        [Required]
        [MaxLength(100)]
        public string NewPassword { get; set; }
    }

    public class AccountPasswordRecoverModel
    {
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }
    }

    public class UserProfileTokenResponse
    {
        public string Token { get; set; }
        public DateTime? TokenExpires { get; set; }
        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
        public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
    }
}
