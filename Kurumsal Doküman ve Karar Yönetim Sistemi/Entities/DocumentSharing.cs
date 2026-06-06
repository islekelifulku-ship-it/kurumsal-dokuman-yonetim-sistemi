namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities
{
    public class DocumentSharing
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public Document? Document { get; set; }

        public int SharedWithUserId { get; set; }
        public User? SharedWithUser { get; set; }

        public int SharedByUserId { get; set; }
        public User? SharedByUser { get; set; }

        public DateTime SharedAt { get; set; } = DateTime.UtcNow;
    }
}
