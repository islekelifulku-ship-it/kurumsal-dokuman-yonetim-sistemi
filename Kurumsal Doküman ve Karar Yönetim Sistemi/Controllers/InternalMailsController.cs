using System;
using System.Linq;
using System.Threading.Tasks;
using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Data;
using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InternalMailsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InternalMailsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("inbox/{userId}")]
        public async Task<IActionResult> GetInbox(int userId)
        {
            var mails = await _context.InternalMails
                .Include(m => m.Sender)
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new
                {
                    m.Id,
                    m.Subject,
                    m.Body,
                    m.IsRead,
                    m.CreatedAt,
                    m.SenderId,
                    SenderName = m.Sender != null ? m.Sender.Name : "Bilinmeyen"
                })
                .ToListAsync();

            return Ok(mails);
        }

        [HttpGet("sent/{userId}")]
        public async Task<IActionResult> GetSent(int userId)
        {
            var mails = await _context.InternalMails
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new
                {
                    m.Id,
                    m.Subject,
                    m.Body,
                    m.IsRead,
                    m.CreatedAt,
                    m.ReceiverId,
                    ReceiverName = m.Receiver != null ? m.Receiver.Name : "Bilinmeyen"
                })
                .ToListAsync();

            return Ok(mails);
        }

        [HttpPost]
        public async Task<IActionResult> SendMail([FromBody] SendMailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body))
            {
                return BadRequest("Konu ve mesaj içeriği boş olamaz.");
            }

            var sender = await _context.Users.FindAsync(request.SenderId);
            var receiver = await _context.Users.FindAsync(request.ReceiverId);

            if (sender == null || receiver == null)
            {
                return BadRequest("Gönderen veya alıcı bulunamadı.");
            }

            var mail = new InternalMail
            {
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                Subject = request.Subject,
                Body = request.Body,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.InternalMails.Add(mail);

            // Create notification for receiver
            var notification = new Notification
            {
                UserId = request.ReceiverId,
                Message = $"Yeni mesaj: {request.Subject} ({sender.Name})",
                Url = "mails.html",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Mesaj başarıyla gönderildi." });
        }

        [HttpPost("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var mail = await _context.InternalMails.FindAsync(id);
            if (mail == null)
            {
                return NotFound("Mesaj bulunamadı.");
            }

            mail.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class SendMailRequest
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
