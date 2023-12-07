using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface ITextTemplateService
    {
        /// <summary>
        /// Create a text template.
        /// </summary>
        /// <param name="item">Text template data</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task<int> CreateAsync(TextTemplateEditDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Delete a text template.
        /// </summary>
        /// <param name="id">Text template ID</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get text templates.
        /// </summary>
        /// <returns></returns>
        SetQuery<TextTemplate> Get();
        /// <summary>
        /// Update a text template.
        /// </summary>
        /// <param name="id">Text template ID</param>
        /// <param name="item">Text template data</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task UpdateAsync(int id, TextTemplateEditDto item, CancellationToken cancellationToken = default);
    }
}
