using Config.Interfaces;
using Core.Exceptions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using MimeKit;
using MimeKit.Text;
using System;
using System.Linq;
using System.Threading.Tasks;
using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPI.Services.Email.Interfaces;

namespace GlobalArticleDatabaseAPI.Services.Email.Implementations
{
    public class EmailSender : IEmailSender
    {
        ISettings _settings { get; }        

        public EmailSender(ISettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task Send(EmailMessage message)
        {
            if (_settings.SmtpEnabled)
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.To.AddRange(message.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                mimeMessage.From.AddRange(message.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                mimeMessage.Subject = message.Subject;
                mimeMessage.Body = new TextPart(TextFormat.Html) // We will say we are sending HTML. But there are options for plaintext etc. 
                {
                    Text = message.Content
                };

                try
                {
                    using (var smtpClient = new SmtpClient())
                    {
                        await smtpClient.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTlsWhenAvailable);

                        await smtpClient.AuthenticateAsync(_settings.SmtpUserName, _settings.SmtpPassword);

                        await smtpClient.SendAsync(mimeMessage);

                        await smtpClient.DisconnectAsync(true);
                    }
                }
                catch (Exception ex)
                {
                    throw new ExceptionBase(ExceptionCodes.EMAIL_SEND_ERROR, ex.Message, ex, StatusCodes.Status500InternalServerError);
                }
            }
        }
    }
}
