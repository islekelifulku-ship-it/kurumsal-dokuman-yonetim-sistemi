using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Data;
using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetLogs()
        {
            var logs = _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(100)
                .Select(a => new {
                    a.Id,
                    a.Action,
                    a.EntityName,
                    a.Details,
                    a.CreatedAt,
                    UserName = a.User != null ? a.User.Name : "Bilinmiyor"
                })
                .ToList();
            
            return Ok(logs);
        }

        [HttpPost]
        public async Task<IActionResult> LogActivity([FromBody] AuditLogRequest request)
        {
            if (string.IsNullOrEmpty(request.Action) || request.UserId <= 0) return BadRequest();

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = request.UserId,
                Action = request.Action,
                EntityName = request.EntityName ?? "Genel",
                Details = request.Details ?? "",
                CreatedAt = System.DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            
            return Ok();
        }
    }

    public class AuditLogRequest
    {
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
