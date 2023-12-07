using Izm.Rumis.Application.Contracts;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Tests.Common
{
    internal class EmailServiceFake : IEmailService
    {
        public MailMessage CreateMessage(string to, string subject, string body, IEnumerable<Attachment> attachments = null)
        {
            return new MailMessage();
        }

        public void Send(MailMessage message)
        {
            // do nothing
        }

        public Task SendAsync(MailMessage message)
        {
            return Task.CompletedTask;
        }
    }
}
