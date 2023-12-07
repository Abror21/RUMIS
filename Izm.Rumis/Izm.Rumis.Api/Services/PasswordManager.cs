using Izm.Rumis.Api.Options;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Infrastructure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Services
{
    public interface IPasswordManager
    {
        /// <summary>
        /// Send a user password recovery email.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        Task RecoverPassword(string email, bool force);
        /// <summary>
        /// Reset user password using a secret key.
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task ResetPassword(string secret, string password);
        /// <summary>
        /// Change known user password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="currentPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        Task ChangePassword(string username, string currentPassword, string newPassword);
    }

    public class PasswordManager : IPasswordManager
    {
        private readonly IUserManager users;
        private readonly IEmailService emails;
        private readonly ITextTemplateService textTemplates;
        private readonly AppSettings options;
        private readonly ILogger<PasswordManager> logger;

        public PasswordManager(IUserManager userManager, IEmailService emailService, ITextTemplateService textTemplateService,
            IOptions<AppSettings> options, ILogger<PasswordManager> logger)
        {
            this.users = userManager;
            this.emails = emailService;
            this.textTemplates = textTemplateService;
            this.options = options.Value;
            this.logger = logger;
        }

        public async Task RecoverPassword(string email, bool force)
        {
            throw new EntityNotFoundException();

            //if (string.IsNullOrEmpty(email) || string.Equals(email, UserNames.Admin, StringComparison.InvariantCultureIgnoreCase))
            //    throw new NotSupportedException("password.resetDisabled");

            //// allow recovering a password only to the forms users
            //var user = users.GetLogins().Where(t => t.UserName == email && t.AuthType == UserAuthType.Forms).First(t => new
            //{
            //    t.Id,
            //    t.UserName,
            //    t.User.IsDisabled
            //});

            //if (user == null)
            //    throw new NotSupportedException("password.resetDisabled");

            //if (user.IsDisabled)
            //    throw new ValidationException("password.cannotResetInactiveUser");

            //try
            //{
            //    var secret = await users.MustResetPassword(user.UserName, force);

            //    var templates = textTemplates.Get().Where(t =>
            //        t.Code == TextTemplateCode.PasswordResetEmailBody
            //        || t.Code == TextTemplateCode.PasswordResetEmailSubject
            //        || t.Code == TextTemplateCode.EmailFooter).List(t => new
            //        {
            //            t.Code,
            //            t.Content
            //        });

            //    var subj = templates.FirstOrDefault(t => t.Code == TextTemplateCode.PasswordResetEmailSubject)?.Content;

            //    if (string.IsNullOrEmpty(subj))
            //        throw new TemplateEmptyException(TextTemplateCode.PasswordResetEmailSubject);

            //    var body = templates.FirstOrDefault(t => t.Code == TextTemplateCode.PasswordResetEmailBody)?.Content;

            //    if (string.IsNullOrEmpty(body))
            //        throw new TemplateEmptyException(TextTemplateCode.PasswordResetEmailBody);

            //    // footer is optional
            //    var footer = templates.FirstOrDefault(t => t.Code == TextTemplateCode.EmailFooter)?.Content;

            //    body = body.Replace("{{url}}", $"{options.AppUrl}/password/reset/{secret}") + footer;

            //    await emails.SendAsync(emails.CreateMessage(email, subj, body));
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, $"Unable to send password recovery email to {email}.");
            //    throw new Exception("password.resetEmailNotSent");
            //}
        }

        public async Task ResetPassword(string secret, string password)
        {
            var loginId = await users.ResetPassword(secret, password);
            var username = users.GetLogins().Where(t => t.Id == loginId).First(t => t.UserName);

            await SendPasswordChangedEmail(username);
        }

        public async Task ChangePassword(string username, string currentPassword, string newPassword)
        {
            var checkPassword = await users.VerifyPassword(username, currentPassword);

            if (!checkPassword)
                throw new ValidationException("password.incorrectCurrentPassword");

            await users.ChangePassword(username, newPassword);
            await SendPasswordChangedEmail(username);
        }

        private async Task SendPasswordChangedEmail(string username)
        {
            throw new NotImplementedException();

            //string email = null;

            //try
            //{
            //    email = users.GetLogins().Where(t => t.UserName == username).First(t => t.UserName);

            //    if (string.IsNullOrEmpty(email))
            //        throw new ValidationException("password.cannotSendNoEmail");

            //    var templates = textTemplates.Get().Where(t =>
            //        t.Code == TextTemplateCode.PasswordChangedEmailBody
            //        || t.Code == TextTemplateCode.PasswordChangedEmailSubject
            //        || t.Code == TextTemplateCode.EmailFooter).List(t => new
            //        {
            //            t.Code,
            //            t.Content
            //        });

            //    var subj = templates.FirstOrDefault(t => t.Code == TextTemplateCode.PasswordChangedEmailSubject)?.Content ?? null;

            //    if (string.IsNullOrEmpty(subj))
            //        throw new TemplateEmptyException(TextTemplateCode.PasswordChangedEmailSubject);

            //    var body = templates.FirstOrDefault(t => t.Code == TextTemplateCode.PasswordChangedEmailBody)?.Content ?? null;

            //    if (string.IsNullOrEmpty(body))
            //        throw new TemplateEmptyException(TextTemplateCode.PasswordChangedEmailBody);

            //    // footer is optional
            //    var footer = templates.FirstOrDefault(t => t.Code == TextTemplateCode.EmailFooter)?.Content;

            //    body = body.Replace("{{url}}", options.AppUrl) + footer;

            //    await emails.SendAsync(emails.CreateMessage(email, subj, body));
            //}
            //catch (Exception ex)
            //{
            //    logger.LogWarning($"Unable to send password change email to {email}.", ex);
            //}
        }
    }
}
