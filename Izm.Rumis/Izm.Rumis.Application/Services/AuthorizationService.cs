using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Models;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IAppDbContext db;
        private readonly ICurrentUserProfileService currentUserProfile;
        private readonly IPersonDataService personDataService;
        private readonly IGdprAuditService gdprAuditService;

        public AuthorizationService(
            IAppDbContext db,
            ICurrentUserProfileService currentUserProfile,
            IPersonDataService personDataService,
            IGdprAuditService gdprAuditService)
        {
            this.db = db;
            this.currentUserProfile = currentUserProfile;
            this.personDataService = personDataService;
            this.gdprAuditService = gdprAuditService;
        }

        /// <inheritdoc />
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void Authorize(IAuthorizedResource item)
        {
            switch (currentUserProfile.Type)
            {
                case UserProfileType.Supervisor:

                    switch (item.PermissionType)
                    {
                        case UserProfileType.Country:
                            throw new UnauthorizedAccessException(Error.PermissionTypeForbidden);

                        case UserProfileType.Supervisor:
                            if (item.SupervisorId != currentUserProfile.SupervisorId)
                                throw new UnauthorizedAccessException(Error.SupervisorForbidden);

                            break;

                        case UserProfileType.EducationalInstitution:
                            var eduInst = db.EducationalInstitutions.FirstOrDefault(t =>
                                                            t.SupervisorId == currentUserProfile.SupervisorId
                                                            && t.Id == item.EducationalInstitutionId.Value);
                            if (eduInst == null)
                                throw new UnauthorizedAccessException(Error.EducationalInstitutionForbidden);

                            break;

                        default:
                            break;
                    }

                    break;

                case UserProfileType.EducationalInstitution:
                    if (item.PermissionType != UserProfileType.EducationalInstitution)
                        throw new UnauthorizedAccessException(Error.PermissionTypeForbidden);

                    if (item.EducationalInstitutionId != currentUserProfile.EducationalInstitutionId)
                        throw new UnauthorizedAccessException(Error.EducationalInstitutionForbidden);

                    break;

                case UserProfileType.Country:
                default:
                    break;
            }
        }

        /// <inheritdoc />
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void Authorize(IAuthorizedResourceCreateDto item)
        {
            if (item.PermissionType == UserProfileType.Supervisor && !item.SupervisorId.HasValue)
                throw new UnauthorizedAccessException(Error.SupervisorIdRequired);

            if (item.PermissionType == UserProfileType.EducationalInstitution && !item.EducationalInstitutionId.HasValue)
                throw new UnauthorizedAccessException(Error.EducationalInstitutionIdRequired);

            switch (currentUserProfile.Type)
            {
                case UserProfileType.Supervisor:

                    switch (item.PermissionType)
                    {
                        case UserProfileType.Country:
                            throw new UnauthorizedAccessException(Error.PermissionTypeForbidden);

                        case UserProfileType.Supervisor:
                            if (item.SupervisorId != currentUserProfile.SupervisorId)
                                throw new UnauthorizedAccessException(Error.SupervisorForbidden);

                            break;

                        case UserProfileType.EducationalInstitution:
                            var eduInst = db.EducationalInstitutions.FirstOrDefault(t =>
                                                            t.SupervisorId == currentUserProfile.SupervisorId
                                                            && t.Id == item.EducationalInstitutionId.Value);
                            if (eduInst == null)
                                throw new UnauthorizedAccessException(Error.EducationalInstitutionForbidden);

                            break;

                        default:
                            break;
                    }

                    break;

                case UserProfileType.EducationalInstitution:
                    if (item.PermissionType != UserProfileType.EducationalInstitution)
                        throw new UnauthorizedAccessException(Error.PermissionTypeForbidden);

                    if (item.EducationalInstitutionId != currentUserProfile.EducationalInstitutionId)
                        throw new UnauthorizedAccessException(Error.EducationalInstitutionForbidden);

                    break;

                case UserProfileType.Country:
                default:
                    break;
            }
        }

        /// <inheritdoc />
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void Authorize(IAuthorizedResourceEditDto item)
        {
            switch (currentUserProfile.Type)
            {
                case UserProfileType.Supervisor:

                    if (item.SupervisorId.HasValue && item.SupervisorId != currentUserProfile.SupervisorId)
                        throw new UnauthorizedAccessException(Error.SupervisorForbidden);

                    if (item.EducationalInstitutionId.HasValue)
                    {
                        var eduInst = db.EducationalInstitutions.FirstOrDefault(t =>
                                                        t.SupervisorId == currentUserProfile.SupervisorId
                                                        && t.Id == item.EducationalInstitutionId.Value);
                        if (eduInst == null)
                            throw new UnauthorizedAccessException(Error.EducationalInstitutionForbidden);
                    }

                    break;

                case UserProfileType.EducationalInstitution:
                    if (item.EducationalInstitutionId != currentUserProfile.EducationalInstitutionId)
                        throw new UnauthorizedAccessException(Error.EducationalInstitutionForbidden);

                    break;

                case UserProfileType.Country:
                default:
                    break;
            }
        }

        /// <inheritdoc />
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void Authorize(IAuthorizedDocumentTemplateEditDto item)
        {
            if (item.PermissionType == UserProfileType.Supervisor && !item.SupervisorId.HasValue)
                throw new UnauthorizedAccessException(Error.SupervisorIdRequired);

            switch (currentUserProfile.Type)
            {
                case (UserProfileType.Supervisor):
                    if (item.SupervisorId != currentUserProfile.SupervisorId)
                        throw new UnauthorizedAccessException(Error.SupervisorForbidden);

                    if (item.PermissionType != UserProfileType.Supervisor)
                        throw new UnauthorizedAccessException(Error.SupervisorForbidden);

                    break;

                case UserProfileType.Country:
                    break;

                default:
                    throw new UnauthorizedAccessException(Error.PermissionTypeForbidden);
            }
        }


        /// <inheritdoc />
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void Authorize(int educationalInstitutionId)
        {
            if (!currentUserProfile.IsInitialized)
                return;

            switch (currentUserProfile.Type)
            {
                case UserProfileType.Supervisor:
                    var hasEduInst = db.EducationalInstitutions.Any(t =>
                                            t.SupervisorId == currentUserProfile.SupervisorId
                                            && t.Id == educationalInstitutionId);

                    if (!hasEduInst)
                        throw new UnauthorizedAccessException(Error.EducationalInstitutionForbidden);

                    break;

                case UserProfileType.EducationalInstitution:
                    if (educationalInstitutionId != currentUserProfile.EducationalInstitutionId)
                        throw new UnauthorizedAccessException(Error.EducationalInstitutionForbidden);

                    break;

                case UserProfileType.Country:
                default:
                    break;
            }
        }

        /// <inheritdoc />
        /// <exception cref="UnauthorizedAccessException"></exception>
        public async Task AuthorizeAsync(string submitterPrivatePersonalIdentifier, string studentPersonalIdentifier, CancellationToken cancellationToken = default)
        {
            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForAuthorizationPersonAccess(studentPersonalIdentifier), cancellationToken);

            if (submitterPrivatePersonalIdentifier == studentPersonalIdentifier)
                return;

            var studentPersonCodes = await personDataService.GetStudentsByParentOrGuardianAsync(submitterPrivatePersonalIdentifier, cancellationToken);

            await gdprAuditService.TraceRangeAsync(studentPersonCodes.Select(GdprAuditHelper.ProjectTraces()).ToArray(), cancellationToken);

            if (!studentPersonCodes.Contains(studentPersonalIdentifier))
                throw new UnauthorizedAccessException(Error.StudentForbidden);
        }

        /// <inheritdoc />
        /// <exception cref="UnauthorizedAccessException"></exception>
        public async Task AuthorizeAsync(string type, string privatePersonalIdentifier, string educationalInstitutionCode, CancellationToken cancellationToken = default)
        {
            await gdprAuditService.TraceAsync(GdprAuditHelper.GenerateTraceForEducationalInstitution(privatePersonalIdentifier, type, educationalInstitutionCode), cancellationToken);

            switch (type)
            {
                case ApplicantRole.Employee:
                    if (!await personDataService.CheckEducationalInstitutionAsEmployee(privatePersonalIdentifier, educationalInstitutionCode, cancellationToken))
                        throw new UnauthorizedAccessException(Error.EducationalInstitutionForbidden);
                    break;
                case ApplicantRole.Learner:
                    if (!await personDataService.CheckEducationalInstitutionAsStudent(privatePersonalIdentifier, educationalInstitutionCode, cancellationToken))
                        throw new UnauthorizedAccessException(Error.EducationalInstitutionForbidden);
                    break;
            }
        }

        public static class Error
        {
            public const string SupervisorIdRequired = "authorization.supervisorIdRequired";
            public const string EducationalInstitutionIdRequired = "authorization.educationalInstitutionIdRequired";
            public const string PermissionTypeForbidden = "authorization.permissionTypeForbidden";
            public const string EducationalInstitutionForbidden = "authorization.educationalInstitutionForbidden";
            public const string SupervisorForbidden = "authorization.supervisorForbidden";
            public const string StudentForbidden = "authorization.studentForbidden";
        }

        public static class GdprAuditHelper
        {
            public static GdprAuditTraceDto GenerateTraceForAuthorizationPersonAccess(string studentPersonalIdentifier)
            {
                return new GdprAuditTraceDto
                {
                    Action = "authorization.authorizeSubmitterAccessToStudent",
                    ActionData = JsonSerializer.Serialize(new { StudentPersonalIdentifier = studentPersonalIdentifier }),
                    DataOwnerId = null,
                    DataOwnerPrivatePersonalIdentifier = studentPersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = studentPersonalIdentifier }
                    }
                };
            }

            public static GdprAuditTraceDto GenerateTraceForEducationalInstitution(string personalIdentifier, string type, string educationalInstitutionCode)
            {
                return new GdprAuditTraceDto
                {
                    Action = "authorization.authorizeAccessToInstitution",
                    ActionData = JsonSerializer.Serialize(new { PersonalIdentifier = personalIdentifier, Type = type, EducationalInstitutionCode = educationalInstitutionCode }),
                    DataOwnerId = null,
                    DataOwnerPrivatePersonalIdentifier = personalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = personalIdentifier }
                    }
                };
            }

            public static Func<string, GdprAuditTraceDto> ProjectTraces()
            {
                return privatePersonalIdentifier => new GdprAuditTraceDto
                {
                    Action = "authorization.authorizeSubmitterAccessToStudent",
                    ActionData = JsonSerializer.Serialize(new { StudentPersonalIdentifier = privatePersonalIdentifier }),
                    DataOwnerId = null,
                    DataOwnerPrivatePersonalIdentifier = privatePersonalIdentifier,
                    Data = new PersonDataProperty[]
                    {
                        new PersonDataProperty { Type = PersonDataType.PrivatePersonalIdentifier, Value = privatePersonalIdentifier }
                    }
                };
            }
        }
    }
}
