using System.Threading.Tasks;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Services
{
    public interface IEmailService
    {
        Task SendDocumentEmailAsync(string toEmail, string subject, string body, string attachmentPath, string attachmentName);
    }
}
