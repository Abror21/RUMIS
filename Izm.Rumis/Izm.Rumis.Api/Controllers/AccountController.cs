using Izm.Rumis.Api.Common;
using Izm.Rumis.Api.Core;
using Izm.Rumis.Api.Extensions;
using Izm.Rumis.Api.Helpers;
using Izm.Rumis.Api.Mappers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Options;
using Izm.Rumis.Api.Services;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    public class AccountController : ApiController
    {
        private readonly AuthSettings authOptions;
        private readonly AuthUserProfileOptions userProfileOptions;
        private readonly ICurrentUserService currentUser;
        private readonly IPasswordManager passwordManager;
        private readonly IUserProfileService userProfileService;
        private readonly IGdprAuditService gdprAuditService;

        public AccountController(
            IOptions<AuthSettings> authOptions,
            IOptions<AuthUserProfileOptions> userProfileOptions,
            ICurrentUserService currentUser,
            IPasswordManager passwordManager,
            IUserProfileService userProfileService,
            IGdprAuditService gdprAuditService)
        {
            this.authOptions = authOptions.Value;
            this.userProfileOptions = userProfileOptions.Value;
            this.currentUser = currentUser;
            this.passwordManager = passwordManager;
            this.userProfileService = userProfileService;
            this.gdprAuditService = gdprAuditService;
        }

        [HttpGet("profiles")]
        public async Task<ActionResult<IEnumerable<UserProfileResponse>>> GetProfiles(CancellationToken cancellationToken = default)
        {
            var data = await userProfileService.GetCurrentUserProfiles()
                .Where(t => !t.Disabled && t.Expires > DateTime.UtcNow)
                .ListAsync(map: UserProfileMapper.ProjectIntermediate(), cancellationToken: cancellationToken);

            await gdprAuditService.TraceRangeAsync(data.Select(GdprAuditHelper.ProjectTraces()), cancellationToken);

            return data.Select(UserProfileMapper.ProjectFromIntermediate())
                .ToArray();
        }

        [HttpPost("setProfile({id})")]
        public async Task<ActionResult<UserProfileTokenResponse>> SetProfile(Guid id, CancellationToken cancellationToken = default)
        {
            var profileData = await userProfileService.GetCurrentUserProfiles()
               .Where(t => t.Id == id)
               .FirstAsync(t => new
               {
                   t.Id,
                   t.Disabled,
                   t.Expires,
                   t.EducationalInstitutionId,
                   EducationalInstitutionSupervisorId = t.EducationalInstitution == null ? null : (int?)t.EducationalInstitution.SupervisorId,
                   t.SupervisorId,
                   t.PermissionType,
                   t.IsLoggedIn,
                   Roles = t.Roles.Select(n => n.Name),
                   Permissions = t.Roles.SelectMany(n => n.Permissions.Select(t => t.Value))
               }, cancellationToken);

            if (profileData == null)
                throw new InvalidOperationException(Error.InvalidUserProfileId);

            if (profileData.Disabled)
                throw new InvalidOperationException(Error.CannotSetDisabledProfile);

            if (profileData.Expires < DateTime.UtcNow)
                throw new InvalidOperationException(Error.CannotSetExpiredProfile);

            var activateTask = userProfileService.ActivateAsync(id, cancellationToken);

            var tokenClaims = new List<Claim>
            {
                ClaimHelper.CreateClaim(ClaimTypes.NameIdentifier, currentUser.Id),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisUserProfileIdentifier, profileData.Id),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisUserProfileType, profileData.PermissionType),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisSessionId, HttpContext.Session.Id),
                ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisSessionCreated, HttpContext.Session.GetString(SessionKey.Created))
            };

            foreach (var role in profileData.Roles)
                tokenClaims.Add(ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisUserProfileRole, role));

            foreach (var permission in profileData.Permissions)
                tokenClaims.Add(ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisUserProfileRolePermission, permission));

            switch (profileData.PermissionType)
            {
                case UserProfileType.EducationalInstitution:
                    tokenClaims.Add(ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisUserProfileEducationalInstitutionIdentifier, profileData.EducationalInstitutionId));
                    tokenClaims.Add(ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisUserProfileSupervisorIdentifier, profileData.EducationalInstitutionSupervisorId));
                    break;
                case UserProfileType.Supervisor:
                    tokenClaims.Add(ClaimHelper.CreateClaim(ClaimTypesExtensions.RumisUserProfileSupervisorIdentifier, profileData.SupervisorId));
                    break;
                case UserProfileType.Country:
                default:
                    break;
            }

            var accessToken = JwtManager.GenerateAccessToken(tokenClaims, userProfileOptions.TokenSecurityKey, profileData.Expires);

            HttpContext.Session.SetString(
                key: SessionKeyHelper.GetUserProfileTokenCreatedSessionKey(profileData.Id),
                value: DateTime.UtcNow.ToBinary().ToString()
                );

            await activateTask;

            return new UserProfileTokenResponse
            {
                Permissions = profileData.Permissions,
                Roles = profileData.Roles,
                Token = accessToken.Token,
                TokenExpires = accessToken.Expires
            };
        }

        [HttpPost("password/reset")]
        public async Task<ActionResult> ResetPassword(AccountPasswordResetModel model)
        {
            if (!authOptions.FormsEnabled)
                return NotFound();

            await passwordManager.ResetPassword(model.Secret, model.Password);
            return NoContent();
        }

        [HttpPost("password/change")]
        public async Task<ActionResult> ChangePassword(AccountPasswordChangeModel model)
        {
            if (!authOptions.FormsEnabled)
                return NotFound();

            await passwordManager.ChangePassword(currentUser.UserName, model.CurrentPassword, model.NewPassword);
            return NoContent();
        }

        [HttpPost("password/recover")]
        [AllowAnonymous]
        public async Task<ActionResult> RecoverPassword(AccountPasswordRecoverModel model)
        {
            if (!authOptions.FormsEnabled)
                return NotFound();

            await passwordManager.RecoverPassword(model.Email, false);
            return NoContent();
        }

        public static class Error
        {
            public const string CannotSetDisabledProfile = "account.cannotSetDisabledProfile";
            public const string CannotSetExpiredProfile = "account.cannotSetExpiredProfile";
            public const string InvalidUserProfileId = "account.invalidUserProfileId";
        }

        public static class GdprAuditHelper
        {
            public static Func<UserProfileIntermediateResponse, GdprAuditTraceDto> ProjectTraces()
            {
                return intermediate => new GdprAuditTraceDto
                {
                    Action = "account.getProfiles",
                    ActionData = null,
                    DataOwnerId = intermediate.PersonTechnicalId,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.Contact, Value = intermediate.Email },
                        new PersonDataProperty { Type = PersonDataType.Contact, Value = intermediate.PhoneNumber }
                    }.Where(t => t.Value != null).ToArray()
                };
            }
        }
    }
}
