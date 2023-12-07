using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Domain.Constants.Classifiers;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Validators
{
    public interface IApplicationValidator
    {
        /// <summary>
        /// Validate <see cref="ApplicationCreateDto"/> data.
        /// </summary>
        /// <param name="item">Data to validate.</param>
        Task ValidateAsync(ApplicationCreateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Validate status change.
        /// </summary>
        /// <param name="entityStatusCode">Entity status code.</param>
        /// <param name="itemStatusCode">Item status code.</param>
        void Validate(string entityStatusCode, string itemStatusCode);
    }

    public sealed class ApplicationValidator : IApplicationValidator
    {
        private readonly IAppDbContext db;

        public ApplicationValidator(IAppDbContext db)
        {
            this.db = db;
        }

        /// <inheritdoc/>
        /// <exception cref="ValidationException"></exception>
        public async Task ValidateAsync(ApplicationCreateDto item, CancellationToken cancellationToken = default)
        {
            if (item.ResourceTargetPerson == null)
                throw new ValidationException(Error.ResourceTargetPersonRequired);

            if (item.SubmitterPerson == null)
                throw new ValidationException(Error.SubmitterPersonRequired);

            //if (!string.IsNullOrEmpty(item.ResourceTargetPerson.FirstName))
            //    throw new ValidationException(Error.ResourceTargetPersonFirstNameRequried);

            //if (!string.IsNullOrEmpty(item.ResourceTargetPerson.LastName))
            //    throw new ValidationException(Error.ResourceTargetPersonLastNameRequried);

            if (!Utility.IsPrivatePersonalIdentifierChecksumValid(item.ResourceTargetPerson.PrivatePersonalIdentifier))
                throw new ValidationException(Error.ResourceTargetPersonInvalidPrivatePersonalIdentifier);

            if (!Utility.IsPrivatePersonalIdentifierChecksumValid(item.SubmitterPerson.PrivatePersonalIdentifier))
                throw new ValidationException(Error.SubmitterPersonInvalidPrivatePersonalIdentifier);

            if (await db.Applications.AnyAsync(t => t.ResourceSubTypeId == item.ResourceSubTypeId
                    && t.ResourceTargetPerson.Persons.Any(p => p.PrivatePersonalIdentifier == item.ResourceTargetPerson.PrivatePersonalIdentifier)
                    && t.EducationalInstitutionId == item.EducationalInstitutionId
                    && (ApplicationStatus.ActiveStatuses.Contains(t.ApplicationStatus.Code)
                            || t.ApplicationResources.Any(ar => PnaStatus.ActiveStatuses.Contains(ar.PNAStatus.Code))), cancellationToken))
            {
                throw new ValidationException(Error.AlreadyExists);
            }
        }

        public void Validate(string entityStatusCode, string itemStatusCode)
        {
            if (entityStatusCode == itemStatusCode)
                return;

            switch (entityStatusCode)
            {
                case ApplicationStatus.Submitted:
                    break;

                case ApplicationStatus.Postponed:
                    if (itemStatusCode == ApplicationStatus.Submitted)
                        throw new ValidationException(Error.StatusChangeForbidden);
                    break;

                default:
                    throw new ValidationException(Error.StatusChangeForbidden);
            }
        }

        public static class Error
        {
            //public const string ResourceTargetPersonFirstNameRequried = "application.resourceTargetPersonFirstNameRequired";
            //public const string ResourceTargetPersonLastNameRequried = "application.resourceTargetPersonLastNameRequired";
            public const string AlreadyExists = "application.alreadyExists";
            public const string ResourceTargetPersonRequired = "application.resourceTargetPersonRequired";
            public const string ResourceTargetPersonInvalidPrivatePersonalIdentifier = "application.resourceTargetPersonInvalidPrivatePersonalIdentifier";
            public const string SubmitterPersonRequired = "application.submitterPersonRequired";
            public const string SubmitterPersonInvalidPrivatePersonalIdentifier = "application.submitterPersonInvalidPrivatePersonalIdentifier";
            public const string StatusChangeForbidden = "application.statusChangeForbidden";
        }
    }
}
