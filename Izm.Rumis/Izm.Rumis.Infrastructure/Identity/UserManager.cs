using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Infrastructure.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Identity
{
    public interface IUserManager
    {
        /// <summary>
        /// Get user logins.
        /// </summary>
        /// <returns></returns>
        SetQuery<IdentityUserLogin> GetLogins();
        /// <summary>
        /// Get users.
        /// </summary>
        /// <returns></returns>
        SetQuery<IdentityUser> GetUsers();
        /// <summary>
        /// Mark a user to reset his password.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="force">If set to true, current user password is removed and user is forced to reset his password using a secret URL.
        /// Otherwise, user must reset his password after next successfull login.</param>
        /// <returns></returns>
        Task<string> MustResetPassword(string username, bool force = false);
        /// <summary>
        /// Reset user password.
        /// </summary>
        /// <param name="secret">Secret key</param>
        /// <param name="password">New password</param>
        /// <returns></returns>
        Task<Guid> ResetPassword(string secret, string password);
        /// <summary>
        /// Change user password.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="password">New password</param>
        /// <returns></returns>
        Task ChangePassword(string username, string password);
        /// <summary>
        /// Verify user password is valid.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="password">User password</param>
        /// <returns></returns>
        Task<bool> VerifyPassword(string username, string password);
        /// <summary>
        /// Get user ID by private personal identifier.
        /// </summary>
        /// <param name="privatePersonalIdentifier">Private personal identifier</param>
        /// <returns>User ID if found, null if not found.</returns>
        Task<Guid?> GetUserIdAsync(string privatePersonalIdentifier, CancellationToken cancellationToken = default);
    }

    public class UserManager : IUserManager
    {
        private readonly IIdentityDbContext db;
        private readonly IPasswordValidator passwordValidator;

        public UserManager(IIdentityDbContext db, IPasswordValidator passwordValidator)
        {
            this.db = db;
            this.passwordValidator = passwordValidator;
        }

        /// <inheritdoc/>
        public async Task<Guid?> GetUserIdAsync(string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            return await db.Persons
                .Where(t => t.PrivatePersonalIdentifier == privatePersonalIdentifier)
                .Select(t => t.PersonTechnical.UserId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public SetQuery<IdentityUserLogin> GetLogins()
        {
            return new SetQuery<IdentityUserLogin>(db.IdentityUserLogins.AsNoTracking());
        }

        /// <inheritdoc/>
        public SetQuery<IdentityUser> GetUsers()
        {
            return new SetQuery<IdentityUser>(db.IdentityUsers.AsNoTracking());
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public async Task<string> MustResetPassword(string username, bool force = false)
        {
            var login = await db.IdentityUserLogins.FirstOrDefaultAsync(t => t.UserName == username);

            if (login == null)
                throw new EntityNotFoundException();

            PreventNonFormsPassword(login);
            PreventAdminUserUpdates(login);

            var secret = Guid.NewGuid().ToString("N");

            if (force)
                login.PasswordHash = null;

            login.PasswordResetKey = secret;
            login.MustResetPassword = true;

            await db.SaveChangesAsync();

            return secret;
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public async Task ChangePassword(string username, string password)
        {
            var login = await db.IdentityUserLogins.FirstOrDefaultAsync(t => t.UserName == username);

            if (login == null)
                throw new EntityNotFoundException();

            PreventNonFormsPassword(login);
            //PreventAdminUserUpdates(login);

            await SetPassword(login, password);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public async Task<Guid> ResetPassword(string secret, string password)
        {
            if (string.IsNullOrEmpty(secret))
                throw new ArgumentNullException("secret");

            var login = await db.IdentityUserLogins.Where(t => t.PasswordResetKey == secret).FirstOrDefaultAsync();

            if (login == null)
                throw new EntityNotFoundException();

            PreventNonFormsPassword(login);
            PreventAdminUserUpdates(login);

            return !login.MustResetPassword ? throw new ValidationException("user.passwordResetDisabled") : await SetPassword(login, password);
        }

        /// <inheritdoc/>
        public async Task<bool> VerifyPassword(string username, string password)
        {
            var user = await db.IdentityUserLogins.Where(t => t.UserName == username).AsNoTracking().FirstOrDefaultAsync();
            return VerifyPassword(user, password);
        }

        private async Task<Guid> SetPassword(IdentityUserLogin user, string password)
        {
            PreventNonFormsPassword(user);

            if (!passwordValidator.Validate(password))
                throw new ValidationException("password.requirementsNotMet");

            var passwordHasher = new PasswordHasher<IdentityUserLogin>();
            var hashedPassword = passwordHasher.HashPassword(user, password);

            user.PasswordResetKey = null;
            user.PasswordHash = hashedPassword;
            user.MustResetPassword = false;

            await db.SaveChangesAsync();

            return user.Id;
        }

        private bool VerifyPassword(IdentityUserLogin user, string password)
        {
            if (user == null || string.IsNullOrEmpty(password))
                return false;

            PreventNonFormsPassword(user);

            var passwordHasher = new PasswordHasher<IdentityUserLogin>();
            var passwordCheck = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            return passwordCheck == PasswordVerificationResult.Success;
        }

        private void PreventAdminUserUpdates(IdentityUserLogin user)
        {
            if (user.UserName == UserNames.Admin)
                throw new NotSupportedException();
        }

        private void PreventNonFormsPassword(IdentityUserLogin user)
        {
            if (user.AuthType != UserAuthType.Forms)
                throw new NotSupportedException();
        }
    }
}
