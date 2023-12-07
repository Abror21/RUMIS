using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IPersonDataService
    {
        /// <summary>
        /// Get birth date.
        /// </summary>
        /// <param name="submitterPersonalIdentifier">Submitter private personal identifier.</param>
        /// <param name="targetPrivatePersonalIdentifier">Taraget private personal identifier.</param>
        /// <param name="type">Applicant role.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns></returns>
        Task<DateTime> GetBirthDateAsync(string submitterPersonalIdentifier, string targetPrivatePersonalIdentifier, string type, CancellationToken cancellation = default);
        /// <summary>
        /// Get students by parent or guardian.
        /// </summary>
        /// <param name="privatePersonalIdentifier">Parent or guardian private personal identifier.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetStudentsByParentOrGuardianAsync(string privatePersonalIdentifier, CancellationToken cancellationToken = default);
        /// <summary>
        /// Check if employee belongs to institution.
        /// </summary>
        /// <param name="privatePersonalIdentifier">Requested person identifier.</param>
        /// <param name="educationalInstitutionCode">Educational institution registration code.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns></returns>
        Task<bool> CheckEducationalInstitutionAsEmployee(string privatePersonalIdentifier, string educationalInstitutionCode, CancellationToken cancellationToken = default);
        /// <summary>
        /// Check if student belongs to institution.
        /// </summary>
        /// <param name="privatePersonalIdentifier">Requested person identifier.</param>
        /// <param name="educationalInstitutionCode">Educational institution registration code.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns></returns>
        Task<bool> CheckEducationalInstitutionAsStudent(string privatePersonalIdentifier, string educationalInstitutionCode, CancellationToken cancellationToken = default);
    }
}
