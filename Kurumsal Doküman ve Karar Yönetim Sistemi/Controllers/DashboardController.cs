using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Data;
using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Genel dashboard istatistikleri (yönetici görünümü)
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalDocuments = await _context.Documents.CountAsync();
            var totalTasks = await _context.TaskItems.CountAsync();
            var completedTasks = await _context.TaskItems.CountAsync(t => t.IsCompleted);
            var pendingTasks = await _context.TaskItems.CountAsync(t => !t.IsCompleted);
            var totalSharings = await _context.DocumentSharings.CountAsync();

            var recentDocuments = await _context.Documents
                .Include(d => d.UploadedByUser)
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.Description,
                    d.OriginalFileName,
                    d.CreatedAt,
                    d.IsExternal,
                    UploadedByUser = d.UploadedByUser == null ? null : new { d.UploadedByUser.Id, d.UploadedByUser.Name, d.UploadedByUser.Email }
                })
                .ToListAsync();

            var recentTasks = await _context.TaskItems
                .Include(t => t.AssignedToUser)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.IsCompleted,
                    t.Status,
                    t.Priority,
                    t.CreatedAt,
                    t.DueDate,
                    AssignedToUser = t.AssignedToUser == null ? null : new { t.AssignedToUser.Id, t.AssignedToUser.Name }
                })
                .ToListAsync();

            var tasksByStatus = await _context.TaskItems
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            var userEfficiency = await _context.Users
                .Where(u => u.Role != "Yönetici") // Sadece personellerin verimliliğini ölç
                .Select(u => new {
                    u.Id,
                    u.Name,
                    TotalTasks = u.Tasks != null ? u.Tasks.Count() : 0,
                    CompletedTasks = u.Tasks != null ? u.Tasks.Count(t => t.IsCompleted) : 0
                })
                .ToListAsync();

            return Ok(new
            {
                totalUsers,
                totalDocuments,
                totalTasks,
                completedTasks,
                pendingTasks,
                totalSharings,
                recentDocuments,
                recentTasks,
                tasksByStatus,
                userEfficiency
            });
        }

        /// <summary>
        /// Belirli bir kullanıcının kendi istatistikleri
        /// </summary>
        [HttpGet("user/{userId}/stats")]
        public async Task<IActionResult> GetUserStats(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            var myDocuments = await _context.Documents
                .Where(d => d.UploadedByUserId == userId)
                .CountAsync();

            var myTasks = await _context.TaskItems
                .Where(t => t.AssignedToUserId == userId)
                .CountAsync();

            var myCompletedTasks = await _context.TaskItems
                .Where(t => t.AssignedToUserId == userId && t.IsCompleted)
                .CountAsync();

            var myPendingTasks = await _context.TaskItems
                .Where(t => t.AssignedToUserId == userId && !t.IsCompleted)
                .CountAsync();

            var sharedWithMe = await _context.DocumentSharings
                .Where(ds => ds.SharedWithUserId == userId)
                .CountAsync();

            var myRecentTasks = await _context.TaskItems
                .Where(t => t.AssignedToUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.IsCompleted,
                    t.Status,
                    t.Priority,
                    t.CreatedAt,
                    t.DueDate
                })
                .ToListAsync();

            return Ok(new
            {
                myDocuments,
                myTasks,
                myCompletedTasks,
                myPendingTasks,
                sharedWithMe,
                myRecentTasks
            });
        }
    }
}
