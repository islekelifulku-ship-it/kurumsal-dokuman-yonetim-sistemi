namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities
{
    public enum GorevDurumu
    {
        Beklemede = 0,
        DevamEdiyor = 1,
        Tamamlandi = 2,
        Iptal = 3
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public GorevDurumu Status { get; set; } = GorevDurumu.Beklemede;
        public string Priority { get; set; } = "Orta"; // Dusuk, Orta, Yuksek
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int? AssignedToUserId { get; set; }
        public User? AssignedToUser { get; set; }
    }
}
