using System;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities
{
    public class Meeting
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime MeetingDate { get; set; }
        public string Agenda { get; set; } = string.Empty; // Gündem / Yapılması Gerekenler
        public string Notes { get; set; } = string.Empty;  // Alınan Notlar
        
        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<MeetingNote>? MeetingNotes { get; set; }
    }
}
