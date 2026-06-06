namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public bool IsExternal { get; set; }
        public int UploadedByUserId { get; set; }
        public User? UploadedByUser { get; set; }
        public ICollection<DocumentSharing>? Sharings { get; set; }
    }
}
