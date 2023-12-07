using Izm.Rumis.Application.Common;
using Izm.Rumis.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IParameterService
    {
        /// <summary>
        /// Get parameters.
        /// </summary>
        /// <returns></returns>
        SetQuery<Parameter> Get();
        /// <summary>
        /// Get parameter value.
        /// </summary>
        /// <param name="code">Parameter code</param>
        /// <returns>Parameter value or null if not found</returns>
        string GetValue(string code);
        /// <summary>
        /// Find parameter value.
        /// </summary>
        /// <param name="code">Parameter code</param>
        /// <param name="parameters">Parameter list</param>
        /// <returns>Parameter value or null if not found</returns>
        string FindValue(string code, IEnumerable<Parameter> parameters);
        /// <summary>
        /// Update a parameter.
        /// </summary>
        /// <param name="id">Parameter ID</param>
        /// <param name="value">Parameter value</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task UpdateAsync(int id, string value, CancellationToken cancellationToken = default);
    }
}
