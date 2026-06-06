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
    public class TaskItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskItemsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTaskItems()
        {
            var tasks = await _context.TaskItems
                .Include(t => t.AssignedToUser)
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
                    t.CompletedAt,
                    t.AssignedToUserId,
                    AssignedToUser = t.AssignedToUser == null ? null : new
                    {
                        t.AssignedToUser.Id,
                        t.AssignedToUser.Name,
                        t.AssignedToUser.Email,
                        t.AssignedToUser.Role
                    }
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskItem(int id)
        {
            var taskItem = await _context.TaskItems
                .Include(t => t.AssignedToUser)
                .Where(t => t.Id == id)
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
                    t.CompletedAt,
                    t.AssignedToUserId,
                    AssignedToUser = t.AssignedToUser == null ? null : new
                    {
                        t.AssignedToUser.Id,
                        t.AssignedToUser.Name,
                        t.AssignedToUser.Email,
                        t.AssignedToUser.Role
                    }
                })
                .FirstOrDefaultAsync();

            if (taskItem == null)
            {
                return NotFound();
            }

            return Ok(taskItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTaskItem(TaskItem taskItem)
        {
            taskItem.CreatedAt = DateTime.UtcNow;
            taskItem.IsCompleted = false;
            taskItem.CompletedAt = null;
            taskItem.Status = GorevDurumu.Beklemede;

            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();

            var currentUserIdStr = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(currentUserIdStr, out int adminId))
            {
                _context.AuditLogs.Add(new AuditLog {
                    UserId = adminId,
                    Action = "CREATE",
                    EntityName = "TaskItem",
                    Details = $"Yeni görev oluşturuldu: {taskItem.Title}"
                });
            }

            if (taskItem.AssignedToUserId.HasValue)
            {
                _context.Notifications.Add(new Notification {
                    UserId = taskItem.AssignedToUserId.Value,
                    Message = $"Size yeni bir görev atandı: {taskItem.Title}",
                    Url = "tasks.html"
                });
            }
            
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskItem), new { id = taskItem.Id }, new { taskItem.Id, taskItem.Title });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaskItem(int id, TaskItem taskItem)
        {
            if (id != taskItem.Id)
            {
                return BadRequest();
            }

            var existingTask = await _context.TaskItems.FindAsync(id);

            if (existingTask == null)
            {
                return NotFound();
            }

            var oldAssignee = existingTask.AssignedToUserId;

            existingTask.Title = taskItem.Title;
            existingTask.Description = taskItem.Description;
            existingTask.Priority = taskItem.Priority;
            existingTask.DueDate = taskItem.DueDate;
            existingTask.AssignedToUserId = taskItem.AssignedToUserId;

            if (taskItem.AssignedToUserId.HasValue && taskItem.AssignedToUserId != oldAssignee)
            {
                _context.Notifications.Add(new Notification {
                    UserId = taskItem.AssignedToUserId.Value,
                    Message = $"Size yeni bir görev atandı: {taskItem.Title}",
                    Url = "tasks.html"
                });
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskItem(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);

            if (taskItem == null)
            {
                return NotFound();
            }

            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/assign/{userId}")]
        public async Task<IActionResult> AssignTask(int id, int userId)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);

            if (taskItem == null)
            {
                return NotFound("Görev bulunamadı.");
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var oldAssignee = taskItem.AssignedToUserId;
            taskItem.AssignedToUserId = userId;

            if (userId != oldAssignee)
            {
                _context.Notifications.Add(new Notification {
                    UserId = userId,
                    Message = $"Size yeni bir görev atandı: {taskItem.Title}",
                    Url = "tasks.html"
                });
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);

            if (taskItem == null)
            {
                return NotFound();
            }

            taskItem.Status = request.Status;

            if (request.Status == GorevDurumu.Tamamlandi && !taskItem.IsCompleted)
            {
                taskItem.IsCompleted = true;
                taskItem.CompletedAt = DateTime.UtcNow;
            }
            else if (request.Status != GorevDurumu.Tamamlandi)
            {
                taskItem.IsCompleted = false;
                taskItem.CompletedAt = null;
            }

            var currentUserIdStr = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(currentUserIdStr, out int currentUserId);

            var admins = await _context.Users.Where(u => u.Role == "Yönetici" && u.Id != currentUserId).ToListAsync();
            foreach (var admin in admins)
            {
                _context.Notifications.Add(new Notification {
                    UserId = admin.Id,
                    Message = $"Görev güncellendi: {taskItem.Title} - Yeni Durum: {request.Status}",
                    Url = "tasks.html"
                });
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteTask(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);

            if (taskItem == null)
            {
                return NotFound();
            }

            if (!taskItem.IsCompleted)
            {
                taskItem.IsCompleted = true;
                taskItem.CompletedAt = DateTime.UtcNow;
                taskItem.Status = GorevDurumu.Tamamlandi;

                await _context.SaveChangesAsync();
            }

            return NoContent();
        }
    }

    public class UpdateStatusRequest
    {
        public GorevDurumu Status { get; set; }
    }
}
