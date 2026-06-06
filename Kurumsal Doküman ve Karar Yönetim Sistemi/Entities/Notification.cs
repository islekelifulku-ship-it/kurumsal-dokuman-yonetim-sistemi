using System;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public User? User { get; set; }
        
        public string Message { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty; // Yönlendirilecek sayfa (örn: tasks.html)
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
