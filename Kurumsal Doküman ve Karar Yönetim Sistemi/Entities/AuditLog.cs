using System;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        
        public string Action { get; set; } = string.Empty; // "CREATE", "UPDATE", "DELETE", "LOGIN"
        public string EntityName { get; set; } = string.Empty; // "Task", "User", "Meeting"
        public string Details { get; set; } = string.Empty; // Detaylı açıklama
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
