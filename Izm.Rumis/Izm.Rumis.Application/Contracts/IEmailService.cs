using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Contracts
{
    public interface IEmailService
    {
        /// <summary>
        /// Create an email message.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        MailMessage CreateMessage(string to, string subject, string body, IEnumerable<Attachment> attachments = null);
        /// <summary>
        /// Send an email message.
        /// </summary>
        /// <param name="message"></param>
        void Send(MailMessage message);
        /// <summary>
        /// Send an email message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendAsync(MailMessage message);
    }
}
