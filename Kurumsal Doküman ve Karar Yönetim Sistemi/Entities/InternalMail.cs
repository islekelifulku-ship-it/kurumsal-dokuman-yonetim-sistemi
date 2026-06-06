using System;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities
{
    public class InternalMail
    {
        public int Id { get; set; }

        public int SenderId { get; set; }
        public User? Sender { get; set; }

        public int ReceiverId { get; set; }
        public User? Receiver { get; set; }

        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
