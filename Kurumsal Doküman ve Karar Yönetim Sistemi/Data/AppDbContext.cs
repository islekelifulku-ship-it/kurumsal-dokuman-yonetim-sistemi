namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Data
{
    using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;

    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<DocumentSharing> DocumentSharings { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MeetingNote> MeetingNotes { get; set; }
        
        public DbSet<Department> Departments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<InternalMail> InternalMails { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MeetingNote -> Meeting
            modelBuilder.Entity<MeetingNote>()
                .HasOne(mn => mn.Meeting)
                .WithMany(m => m.MeetingNotes)
                .HasForeignKey(mn => mn.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            // MeetingNote -> User
            modelBuilder.Entity<MeetingNote>()
                .HasOne(mn => mn.User)
                .WithMany()
                .HasForeignKey(mn => mn.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Department
            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // AuditLog -> User
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification -> User
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Meeting -> CreatedByUser
            modelBuilder.Entity<Meeting>()
                .HasOne(m => m.CreatedByUser)
                .WithMany()
                .HasForeignKey(m => m.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // DocumentSharing -> Document
            modelBuilder.Entity<DocumentSharing>()
                .HasOne(ds => ds.Document)
                .WithMany(d => d.Sharings)
                .HasForeignKey(ds => ds.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // DocumentSharing -> SharedWithUser
            modelBuilder.Entity<DocumentSharing>()
                .HasOne(ds => ds.SharedWithUser)
                .WithMany()
                .HasForeignKey(ds => ds.SharedWithUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // DocumentSharing -> SharedByUser
            modelBuilder.Entity<DocumentSharing>()
                .HasOne(ds => ds.SharedByUser)
                .WithMany()
                .HasForeignKey(ds => ds.SharedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskItem -> AssignedToUser
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.AssignedToUser)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Document -> UploadedByUser
            modelBuilder.Entity<Document>()
                .HasOne(d => d.UploadedByUser)
                .WithMany()
                .HasForeignKey(d => d.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // InternalMail -> Sender
            modelBuilder.Entity<InternalMail>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // InternalMail -> Receiver
            modelBuilder.Entity<InternalMail>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
