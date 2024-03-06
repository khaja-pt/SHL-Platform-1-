using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace SHL_Platform.Services
{
    public class EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        private readonly SmtpSettings _smtpSettings = smtpSettings.Value;

        public async Task SendEmailAsync(string to, string subject, string body, Attachment attachment = null, string bcc = null, string cc = null)
        {

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.SmtpUsername),
                Subject = subject,
                IsBodyHtml = true,
                Body = body // Set the HTML body directly here
            };

            // Attach the attachment if provided
            if (attachment != null)
            {
                // Set Content-ID of the attachment
                attachment.ContentId = Guid.NewGuid().ToString();
                mailMessage.Attachments.Add(attachment);
            }
            if (to != null)
            {
                List<string> recipients = [.. to.Split(';')];
                foreach (string recipient in recipients)
                {
                    mailMessage.To.Add(recipient);
                }
            }
            if (bcc != null)
            {
                List<string> bccList = [.. bcc.Split(';')];
                foreach (string recipient in bccList)
                {
                    mailMessage.Bcc.Add(recipient);
                }
            }
            if (cc != null)
            {
                List<string> ccList = [.. cc.Split(';')];
                foreach (string recipient in ccList)
                {
                    mailMessage.CC.Add(recipient);
                }
            }
            using var smtpClient = new SmtpClient(_smtpSettings.SmtpServer, _smtpSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_smtpSettings.SmtpUsername, _smtpSettings.SmtpPassword),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
