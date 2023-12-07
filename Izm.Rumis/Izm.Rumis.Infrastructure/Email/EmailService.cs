using Izm.Rumis.Application.Contracts;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Email
{
    public class EmailServiceOptions
    {
        public bool EnableSsl { get; set; } = true;
        public string From { get; set; }
        public string Password { get; set; }
        public int Port { get; set; } = 25;
        public string Server { get; set; }
        public string Username { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly EmailServiceOptions options;

        public EmailService(IOptions<EmailServiceOptions> options)
        {
            this.options = options.Value;
        }

        /// <inheritdoc/>
        public MailMessage CreateMessage(string to, string subject, string body, IEnumerable<Attachment> attachments = null)
        {
            var message = new MailMessage(options.From, to)
            {
                IsBodyHtml = false,
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8
            };

            if (attachments != null)
                foreach (var attachment in attachments)
                    message.Attachments.Add(attachment);

            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(body, new ContentType(MediaTypeNames.Text.Plain)
            {
                CharSet = Encoding.UTF8.WebName
            }));

            //message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(body, new ContentType(MediaTypeNames.Text.Html)
            //{
            //    CharSet = Encoding.UTF8.WebName
            //}));

            return message;
        }

        /// <inheritdoc/>
        public void Send(MailMessage message)
        {
            using var client = CreateClient();

            client.Send(message);

            message.Dispose();
        }

        /// <inheritdoc/>
        public async Task SendAsync(MailMessage message)
        {
            using var client = CreateClient();

            await client.SendMailAsync(message);

            message.Dispose();
        }

        private SmtpClient CreateClient()
        {
            var client = new SmtpClient(options.Server, options.Port)
            {
                EnableSsl = options.EnableSsl
            };

            if (!string.IsNullOrEmpty(options.Username) && !string.IsNullOrEmpty(options.Password))
                client.Credentials = new NetworkCredential(options.Username, options.Password);

            return client;
        }
    }
}
