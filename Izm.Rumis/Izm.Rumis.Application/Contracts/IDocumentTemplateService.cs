using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IDocumentTemplateService
    {
        /// <summary>
        /// Create a document template.
        /// </summary>
        /// <param name="item">Document template data</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task<int> CreateAsync(DocumentTemplateEditDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Delete a document template.
        /// </summary>
        /// <param name="id">Document template ID</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get document templates.
        /// </summary>
        /// <returns></returns>
        SetQuery<DocumentTemplate> Get();
        /// <summary>
        /// Get document templates by educational institution.
        /// </summary>
        /// <param name="eduInstId">Educational institution ID</param>
        /// <returns></returns>
        SetQuery<DocumentTemplate> GetByEducationalInstitution(int eduInstId);
        /// <summary>
        /// Get document template sample html string.
        /// </summary>
        /// <param name="id">Document template ID.</param
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Document template html string.</returns>
        Task<string> GetSampleAsync(int id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update a document template.
        /// </summary>
        /// <param name="id">Document template ID</param>
        /// <param name="item">Document template data</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task UpdateAsync(int id, DocumentTemplateEditDto item, CancellationToken cancellationToken = default);
    }
}
