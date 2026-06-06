using System;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities
{
    public class MeetingNote
    {
        public int Id { get; set; }
        public string NoteText { get; set; } = string.Empty;
        
        public int MeetingId { get; set; }
        public Meeting? Meeting { get; set; }
        
        public int UserId { get; set; }
        public User? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
