using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendDocumentEmailAsync(string toEmail, string subject, string body, string attachmentPath, string attachmentName)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var senderEmail = emailSettings["SenderEmail"] ?? "ornek@sirket.com";
            // Gmail App Password (Uygulama Şifresi) kullanılmalıdır
            var senderPassword = emailSettings["SenderPassword"] ?? "varsayilan_sifre"; 
            var smtpHost = emailSettings["SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, "KDKYS Sistem"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            
            mailMessage.To.Add(toEmail);

            if (!string.IsNullOrEmpty(attachmentPath) && System.IO.File.Exists(attachmentPath))
            {
                var attachment = new Attachment(attachmentPath);
                attachment.Name = attachmentName;
                mailMessage.Attachments.Add(attachment);
            }

            await client.SendMailAsync(mailMessage);
        }
    }
}
