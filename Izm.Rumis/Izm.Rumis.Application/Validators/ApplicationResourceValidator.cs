using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Validators
{
    public interface IApplicationResourceValidator
    {
        /// <summary>
        /// Validate <see cref="ApplicationResourceCreateDto"/> data.
        /// </summary>
        /// <param name="item">Data to validate.</param>
        Task ValidateAsync(ApplicationResourceCreateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Validate <see cref="ApplicationResourceReturnEditDto"/> data.
        /// </summary>
        /// <param name="item">Data to validate.</param>
        Task ValidateResourceStatusAsync(ApplicationResourceReturnEditDto item, CancellationToken cancellationToken = default);
    }

    public sealed class ApplicationResourceValidator : IApplicationResourceValidator
    {
        private readonly IAppDbContext db;
        private readonly ICurrentUserProfileService currentUserProfile;

        public ApplicationResourceValidator(IAppDbContext db, ICurrentUserProfileService currentUserProfile)
        {
            this.db = db;
            this.currentUserProfile = currentUserProfile;
        }

        /// <inheritdoc/>
        /// <exception cref="ValidationException"></exception>
        public async Task ValidateAsync(ApplicationResourceCreateDto item, CancellationToken cancellationToken = default)
        {
            var application = await db.Applications.FindAsync(new object[] { item.ApplicationId }, cancellationToken);

            switch (application.ApplicationStatus.Code)
            {
                case ApplicationStatus.Submitted:
                    if (application.ApplicationResources.Any())
                        throw new ValidationException(Error.CountExceeded);

                    break;

                case ApplicationStatus.Postponed:
                    if (application.ApplicationResources.Any())
                        throw new ValidationException(Error.CountExceeded);

                    break;

                case ApplicationStatus.Confirmed:
                    if (!currentUserProfile.Permissions.Contains(Permission.ApplicationResourceReassign))
                        throw new ValidationException(Error.ReassignForbidden);

                    if (application.ApplicationResources.Any(t => t.PNAStatus.Code == PnaStatus.Preparing
                                                                    || t.PNAStatus.Code == PnaStatus.Prepared
                                                                    || t.PNAStatus.Code == PnaStatus.Issued))
                    {
                        throw new ValidationException(Error.ReassignForbidden);
                    }


                    break;

                default:
                    throw new ValidationException(Error.CreationForbidden);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ValidationException"></exception>
        public async Task ValidateResourceStatusAsync(ApplicationResourceReturnEditDto item, CancellationToken cancellationToken = default)
        {
            var resourceStatusIdCheck = await db.Classifiers
                .Where(t => t.Type == ClassifierTypes.ResourceStatus
                        && (t.Code == ResourceStatus.Available || t.Code == ResourceStatus.Damaged || t.Code == ResourceStatus.Maintenance))
                .AnyAsync(t => t.Id == item.ResourceStatusId);

            if (!resourceStatusIdCheck)
                throw new ValidationException(Error.AssignedResourceInvalidResourceStatus);
        }

        public static class Error
        {
            public const string CountExceeded = "applicationResource.countExceeded";
            public const string CreationForbidden = "applicationResource.creationForbidden";
            public const string ReassignForbidden = "applicationResource.reassignForbidden";
            public const string AssignedResourceInvalidResourceStatus = "applicationResource.assignedResourceInvalidResourceStatus";
        }
    }
}
