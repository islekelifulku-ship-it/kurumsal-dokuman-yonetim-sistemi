using Microsoft.AspNetCore.Http;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities
{
    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; }

        public string Title { get; set; } = string.Empty;

        public int UploadedByUserId { get; set; }

        public bool IsExternal { get; set; }
    }
}
