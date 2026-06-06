using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Data;
using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetUserNotifications(int userId)
        {
            var notifications = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .ToList();
            
            return Ok(notifications);
        }

        [HttpPut("{id}/read")]
        public IActionResult MarkAsRead(int id)
        {
            var notif = _context.Notifications.Find(id);
            if (notif == null) return NotFound();

            notif.IsRead = true;
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost("mark-all-read/user/{userId}")]
        public IActionResult MarkAllAsRead(int userId)
        {
            var notifs = _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToList();
            foreach (var n in notifs) n.IsRead = true;
            _context.SaveChanges();
            return Ok();
        }
    }
}
