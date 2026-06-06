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
    public class DocumentSharingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DocumentSharingController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Bir dokümanı belirli bir kullanıcıyla paylaş
        /// </summary>
        [HttpPost("share")]
        public async Task<IActionResult> ShareDocument([FromBody] ShareDocumentRequest request)
        {
            var document = await _context.Documents.FindAsync(request.DocumentId);
            if (document == null)
                return NotFound("Doküman bulunamadı.");

            var targetUser = await _context.Users.FindAsync(request.SharedWithUserId);
            if (targetUser == null)
                return NotFound("Hedef kullanıcı bulunamadı.");

            var sharedByUser = await _context.Users.FindAsync(request.SharedByUserId);
            if (sharedByUser == null)
                return NotFound("Paylaşan kullanıcı bulunamadı.");

            // Zaten paylaşılmış mı kontrol et
            var exists = await _context.DocumentSharings
                .AnyAsync(ds => ds.DocumentId == request.DocumentId
                             && ds.SharedWithUserId == request.SharedWithUserId);
            if (exists)
                return BadRequest("Bu doküman zaten bu kullanıcıyla paylaşılmış.");

            var sharing = new DocumentSharing
            {
                DocumentId = request.DocumentId,
                SharedWithUserId = request.SharedWithUserId,
                SharedByUserId = request.SharedByUserId,
                SharedAt = DateTime.UtcNow
            };

            _context.DocumentSharings.Add(sharing);

            _context.Notifications.Add(new Notification {
                UserId = request.SharedWithUserId,
                Message = $"{sharedByUser.Name} sizinle bir doküman paylaştı: {document.Title}",
                Url = "documents.html"
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Doküman başarıyla paylaşıldı.", sharingId = sharing.Id });
        }

        /// <summary>
        /// Paylaşımı kaldır
        /// </summary>
        [HttpDelete("{sharingId}")]
        public async Task<IActionResult> RemoveSharing(int sharingId)
        {
            var sharing = await _context.DocumentSharings.FindAsync(sharingId);
            if (sharing == null)
                return NotFound();

            _context.DocumentSharings.Remove(sharing);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Belirli bir dokümanın paylaşım listesi
        /// </summary>
        [HttpGet("document/{documentId}")]
        public async Task<IActionResult> GetDocumentSharings(int documentId)
        {
            var sharings = await _context.DocumentSharings
                .Where(ds => ds.DocumentId == documentId)
                .Include(ds => ds.SharedWithUser)
                .Include(ds => ds.SharedByUser)
                .Select(ds => new
                {
                    ds.Id,
                    ds.DocumentId,
                    ds.SharedAt,
                    SharedWithUser = new { ds.SharedWithUser!.Id, ds.SharedWithUser.Name, ds.SharedWithUser.Email },
                    SharedByUser = new { ds.SharedByUser!.Id, ds.SharedByUser.Name, ds.SharedByUser.Email }
                })
                .ToListAsync();

            return Ok(sharings);
        }

        /// <summary>
        /// Bir kullanıcıyla paylaşılan dokümanlar
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetSharedWithUser(int userId)
        {
            var sharings = await _context.DocumentSharings
                .Where(ds => ds.SharedWithUserId == userId)
                .Include(ds => ds.Document)
                    .ThenInclude(d => d!.UploadedByUser)
                .Include(ds => ds.SharedByUser)
                .Select(ds => new
                {
                    ds.Id,
                    ds.SharedAt,
                    SharedByUser = new { ds.SharedByUser!.Id, ds.SharedByUser.Name },
                    Document = new
                    {
                        ds.Document!.Id,
                        ds.Document.Title,
                        ds.Document.Description,
                        ds.Document.OriginalFileName,
                        ds.Document.CreatedAt,
                        ds.Document.IsExternal
                    }
                })
                .ToListAsync();

            return Ok(sharings);
        }
    }

    public class ShareDocumentRequest
    {
        public int DocumentId { get; set; }
        public int SharedWithUserId { get; set; }
        public int SharedByUserId { get; set; }
    }
}
