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
    public class DocumentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DocumentsController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> GetDocuments()
        {
            var documents = await _context.Documents
                .Include(d => d.UploadedByUser)
                .Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.Description,
                    d.OriginalFileName,
                    d.CreatedAt,
                    d.IsExternal,
                    d.UploadedByUserId,
                    UploadedByUser = d.UploadedByUser == null ? null : new
                    {
                        d.UploadedByUser.Id,
                        d.UploadedByUser.Name,
                        d.UploadedByUser.Email,
                        d.UploadedByUser.Role
                    }
                })
                .ToListAsync();

            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(int id)
        {
            var document = await _context.Documents
                .Include(d => d.UploadedByUser)
                .Where(d => d.Id == id)
                .Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.Description,
                    d.OriginalFileName,
                    d.CreatedAt,
                    d.IsExternal,
                    d.UploadedByUserId,
                    UploadedByUser = d.UploadedByUser == null ? null : new
                    {
                        d.UploadedByUser.Id,
                        d.UploadedByUser.Name,
                        d.UploadedByUser.Email,
                        d.UploadedByUser.Role
                    }
                })
                .FirstOrDefaultAsync();

            if (document == null)
                return NotFound();

            return Ok(document);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDocument(Document document)
        {
            document.CreatedAt = DateTime.UtcNow;
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, new { document.Id, document.Title });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, Document document)
        {
            if (id != document.Id)
                return BadRequest();

            var existing = await _context.Documents.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Title = document.Title;
            existing.Description = document.Description;
            existing.IsExternal = document.IsExternal;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            if (!string.IsNullOrEmpty(document.FilePath) && System.IO.File.Exists(document.FilePath))
                System.IO.File.Delete(document.FilePath);

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument(
            IFormFile file,
            [FromForm] string title,
            [FromForm] string? description,
            [FromForm] int uploadedByUserId,
            [FromForm] bool isExternal)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".doc", ".xls", ".txt", ".png", ".jpg" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Desteklenmeyen dosya türü.");

            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var document = new Document
            {
                Title = title,
                Description = description ?? string.Empty,
                OriginalFileName = file.FileName,
                FilePath = filePath,
                CreatedAt = DateTime.UtcNow,
                UploadedByUserId = uploadedByUserId,
                IsExternal = isExternal
            };

            _context.Documents.Add(document);
            
            var admins = await _context.Users.Where(u => u.Role == "Yönetici" && u.Id != uploadedByUserId).ToListAsync();
            foreach (var admin in admins)
            {
                _context.Notifications.Add(new Notification {
                    UserId = admin.Id,
                    Message = $"Sisteme yeni bir doküman yüklendi: {document.Title}",
                    Url = "documents.html"
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                document.Id,
                document.Title,
                document.Description,
                document.OriginalFileName,
                document.CreatedAt,
                document.IsExternal,
                document.UploadedByUserId
            });
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            if (!System.IO.File.Exists(document.FilePath))
                return NotFound("Dosya bulunamadı.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(document.FilePath);

            var ext = Path.GetExtension(document.FilePath).ToLower();
            var contentType = ext switch
            {
                ".pdf"  => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".doc"  => "application/msword",
                ".xls"  => "application/vnd.ms-excel",
                ".txt"  => "text/plain",
                ".png"  => "image/png",
                ".jpg"  => "image/jpeg",
                _       => "application/octet-stream"
            };

            var downloadName = string.IsNullOrEmpty(document.OriginalFileName)
                ? Path.GetFileName(document.FilePath)
                : document.OriginalFileName;

            return File(fileBytes, contentType, downloadName);
        }

        [HttpPost("{id}/share-email")]
        public async Task<IActionResult> ShareViaEmail(int id, [FromBody] EmailShareRequest request, [FromServices] Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Services.IEmailService emailService)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound("Doküman bulunamadı.");

            if (!System.IO.File.Exists(document.FilePath))
                return NotFound("Dosya fiziksel olarak bulunamadı.");

            try
            {
                var subject = $"KDKYS Doküman Paylaşımı: {document.Title}";
                var body = $"<div style='font-family:sans-serif; padding:20px; color:#333;'>" +
                           $"<h2 style='color:#6366f1;'>KDKYS Doküman Paylaşımı</h2>" +
                           $"<p>Merhaba,</p>" +
                           $"<p>Kurumsal Doküman ve Karar Yönetim Sistemi'nden sizinle bir doküman paylaşıldı:</p>" +
                           $"<div style='background:#f4f4f5; padding:15px; border-left:4px solid #6366f1; margin:15px 0;'>" +
                           $"<strong>Doküman Adı:</strong> {document.Title}<br/>" +
                           $"<strong>Mesaj:</strong> {request.Message}" +
                           $"</div>" +
                           $"<p>Doküman e-postanın ekinde yer almaktadır.</p>" +
                           $"<p style='color:#999; font-size:12px; margin-top:30px;'>Bu e-posta otomatik olarak gönderilmiştir.</p>" +
                           $"</div>";
                
                await emailService.SendDocumentEmailAsync(request.Email, subject, body, document.FilePath, document.OriginalFileName);
                
                var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (targetUser != null)
                {
                    _context.Notifications.Add(new Notification {
                        UserId = targetUser.Id,
                        Message = $"E-posta üzerinden size bir doküman gönderildi: {document.Title}",
                        Url = "documents.html"
                    });
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "E-posta başarıyla gönderildi." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"E-posta gönderilirken hata oluştu: {ex.Message}");
            }
        }
    }

    public class EmailShareRequest
    {
        public string Email { get; set; }
        public string Message { get; set; }
    }
}