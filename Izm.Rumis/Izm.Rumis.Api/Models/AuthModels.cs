using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Api.Models
{
    public class AuthTicketSignInModel
    {
        [Required]
        public string Ticket { get; set; }

        [Required]
        public string Type { get; set; }
    }

    public class AuthTicketSignInResponse
    {
        public string RedirectUrl { get; set; }
    }

    public class AuthConfigurationResponse
    {
        public bool FormsEnabled { get; set; }
        public bool ExternalEnabled { get; set; }

        public string ExternalUrl { get; set; }
        public string ExternalLogoutUrl { get; set; }

        public int SessionIdleTimeoutInMinutes { get; set; }
        public int NotifyBeforeTimeoutInMinutes { get; set; }

        public PasswordSettings PasswordSettings { get; set; }
    }

    public class AuthLoginModel
    {
        public string Code { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class AuthUserResponse
    {
        public Guid Id { get; set; }
        public Guid? PersonId { get; set; }
        public string AccessToken { get; set; }
        public DateTime? AccessTokenExpires { get; set; }
        public string UserName { get; set; }
        public UserAuthType AuthType { get; set; }

        public IEnumerable<PersonData> Persons { get; set; } = Array.Empty<PersonData>();

        public AuthUserProfile Profile { get; set; }

        internal Guid LoginId { get; set; }
        internal bool IsDisabled { get; set; }
        internal bool IsHidden { get; set; }
        internal bool MustResetPassword { get; set; }

        public class AuthUserProfile
        {
            public Guid Id { get; set; }
            public UserProfileType Type { get; set; }
            public int? SupervisorId { get; set; }
            public string Role { get; set; }
            public int? EducationalInstitutionId { get; set; }

            public IEnumerable<string> Permissions { get; set; } = new List<string>();
        }

        public class PersonData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            //public string PrivatePersonalIdentifier { get; set; }
        }
    }

    public class AuthLogoutResponseModel
    {
        public string RedirectUrl { get; set; }
    }

    public class AuthRegisterModel
    {
        public UserAuthType AuthType { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; } = new string[] { };
    }
}
