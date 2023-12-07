using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Events.UserProfile;
using Izm.Rumis.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Izm.Rumis.Domain.Entities
{
    public class UserProfile : Entity<Guid>, IAuthorizedResource
    {
        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [MaxLength(50)]
        public string Job { get; set; }
        public bool Disabled { get; protected set; }
        public DateTime Expires { get; protected set; }
        public UserProfileType PermissionType { get; protected set; }
        public bool IsLoggedIn { get; set; }

        [MaxLength(100)]
        public string ProfileCreationDocumentNumber { get; set; }
        public DateTime? ProfileCreationDocumentDate { get; set; }

        [Column(TypeName = "longtext")]
        public string Notes { get; set; }

        [Column(TypeName = "longtext")]
        public string ConfigurationInfo { get; set; }

        public int? EducationalInstitutionId { get; protected set; }
        public virtual EducationalInstitution EducationalInstitution { get; protected set; }

        public int? SupervisorId { get; protected set; }
        public virtual Supervisor Supervisor { get; protected set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        [ClassifierType(ClassifierTypes.Institution)]
        public Guid? InstitutionId { get; set; }
        public virtual Classifier Institution { get; set; }

        public virtual ICollection<Role> Roles { get; private set; } = new List<Role>();

        private bool IsInCreation => Events.Any(e => typeof(UserProfileCreatedEvent) == e.GetType());

        protected UserProfile()
        {
            Id = Guid.NewGuid();
        }

        public void ClearRoles()
        {
            var roles = Roles.ToArray();

            foreach (var role in roles)
                Roles.Remove(role);
        }

        public static UserProfile Create()
        {
            var userProfile = new UserProfile();

            userProfile.Events.Add(new UserProfileCreatedEvent(userProfile));

            return userProfile;
        }

        public void Disable()
        {
            if (Disabled)
                return;

            Disabled = true;

            Events.Add(new UserProfileDisabledEvent(Id));
        }

        public void Enable()
        {
            if (!Disabled)
                return;

            Disabled = false;

            Events.Add(new UserProfileEnabledEvent(Id));
        }

        public void SetAccessLevel(AccessLevel accessLevel)
        {
            int? newSupervisorId = accessLevel.Type == UserProfileType.Supervisor ? accessLevel.SupervisorId : null;
            int? newEducationalInstitutionId = accessLevel.Type == UserProfileType.EducationalInstitution ? accessLevel.EducationalInstitutionId : null;

            if (PermissionType == accessLevel.Type
                && SupervisorId == newSupervisorId
                && EducationalInstitutionId == newEducationalInstitutionId)
                return;

            PermissionType = accessLevel.Type;
            EducationalInstitutionId = newEducationalInstitutionId;
            SupervisorId = newSupervisorId;

            Events.Add(new UserProfileAccessLevelChangedEvent(Id, accessLevel));
        }

        public void SetDisabled(bool disabled)
        {
            if (disabled)
                Disable();
            else
                Enable();
        }

        public void SetExpiration(DateTime expires)
        {
            if (Expires == expires)
                return;

            Expires = expires;

            if (!IsInCreation)
                Events.Add(new UserProfileExpirationChangedEvent(Id, expires));
        }

        public void SetRoles(IEnumerable<Role> roles)
        {
            var rolesToRemove = Roles.Where(role => !roles.Any(t => t.Id == role.Id))
                .ToArray();

            var rolesToAdd = roles.Where(role => !Roles.Any(t => t.Id == role.Id))
                .ToArray();

            if (!rolesToAdd.Any() && !rolesToRemove.Any())
                return;

            foreach (var role in rolesToRemove)
                Roles.Remove(role);

            foreach (var role in rolesToAdd)
                Roles.Add(role);

            if (!IsInCreation)
                Events.Add(new UserProfileRolesChangedEvent(Id));
        }
    }
}
