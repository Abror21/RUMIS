using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IUserProfileService
    {
        /// <summary>
        /// Activate a user profile.
        /// </summary>
        /// <param name="id">User profile ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Create a user profile.
        /// </summary>
        /// <param name="item">User profile creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created user profile ID.</returns>
        Task<Guid> CreateAsync(UserProfileEditDto item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Delete a user profile.
        /// </summary>
        /// <param name="id">User profile ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Query user profiles.
        /// </summary>
        /// <returns>User profiles query wrapped in <see cref="SetQuery{T}"/>.</returns>
        SetQuery<UserProfile> Get();
        /// <summary>
        /// Get current user user profiles.
        /// </summary>
        /// <returns>User profiles query wrapped in <see cref="SetQuery{T}"/>.</returns>
        SetQuery<UserProfile> GetCurrentUserProfiles();
        /// <summary>
        /// Update a user profile.
        /// </summary>
        /// <param name="id">User profile ID.</param>
        /// <param name="item">User profile update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task UpdateAsync(Guid id, UserProfileEditDto item, CancellationToken cancellationToken = default);
    }
}
