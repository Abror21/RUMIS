using Izm.Rumis.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IAuthorizationService
    {
        /// <summary>
        /// Authorize <see cref="IAuthorizedResource"/> item.
        /// </summary>
        /// <param name="item">Item to authorize</param>
        /// <returns></returns>
        void Authorize(IAuthorizedResource item);
        /// <summary>
        /// Authorize <see cref="IAuthorizedResourceCreateDto"/> item.
        /// </summary>
        /// <param name="item">Item to authorize</param>
        /// <returns></returns>
        void Authorize(IAuthorizedResourceCreateDto item);
        /// <summary>
        /// Authorize <see cref="IAuthorizedResourceEditDto"/> item.
        /// </summary>
        /// <param name="item">Item to authorize</param>
        /// <returns></returns>
        void Authorize(IAuthorizedResourceEditDto item);
        /// <summary>
        /// Authorize <see cref="IAuthorizedDocumentTemplateEditDto"/> item.
        /// </summary>
        /// <param name="item">Item to authorize</param>
        /// <returns></returns>
        void Authorize(IAuthorizedDocumentTemplateEditDto item);
        /// <summary>
        /// Authorize educational institution id.
        /// </summary>
        /// <param name="educationalInstitutionId">Educational institution id to authorize</param>
        /// <returns></returns>
        void Authorize(int educationalInstitutionId);
        /// <summary>
        /// Authorize submitter against student.
        /// </summary>
        /// <param name="submitterPrivatePersonalIdentifier">Submitter private personal identifier.</param>
        /// <param name="studentPersonalIdentifier">Student personal identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task AuthorizeAsync(string submitterPrivatePersonalIdentifier, string studentPersonalIdentifier, CancellationToken cancellationToken = default);
        /// <summary>
        /// Authorize submitter against educational institution.
        /// </summary>
        /// <param name="type">Employee of student (learner).</param>
        /// <param name="privatePersonalIdentifier">Requsted person personal identifier.</param>
        /// <param name="educationalInstitution">Educational institution registration code.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task AuthorizeAsync(string type, string privatePersonalIdentifier, string educationalInstitution, CancellationToken cancellationToken = default);
    }
}
