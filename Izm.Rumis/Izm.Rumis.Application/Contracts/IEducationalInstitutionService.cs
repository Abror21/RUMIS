using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IEducationalInstitutionService
    {
        /// <summary>
        /// Create a EducationalInstitution.
        /// </summary>
        /// <param name="item">EducationalInstitution creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created EducationalInstitution ID.</returns>
        Task<int> CreateAsync(EducationalInstitutionCreateDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Delete a EducationalInstitution.
        /// </summary>
        /// <param name="id">EducationalInstitution ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Query EducationalInstitutions.
        /// </summary>
        /// <returns>EducationalInstitutions query wrapped in <see cref="SetQuery{T}"/>.</returns>
        SetQuery<EducationalInstitution> Get();
        /// <summary>
        /// Update a EducationalInstitution.
        /// </summary>
        /// <param name="id">EducationalInstitution ID.</param>
        /// <param name="item">EducationalInstitution update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task UpdateAsync(int id, EducationalInstitutionUpdateDto item, CancellationToken cancellationToken = default);
    }
}
