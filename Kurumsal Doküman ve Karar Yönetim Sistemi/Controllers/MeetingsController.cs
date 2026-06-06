using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Data;
using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MeetingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMeetings()
        {
            var meetings = await _context.Meetings
                .Include(m => m.CreatedByUser)
                .Include(m => m.MeetingNotes!)
                    .ThenInclude(mn => mn.User)
                .OrderBy(m => m.MeetingDate)
                .Select(m => new {
                    m.Id,
                    m.Title,
                    m.MeetingDate,
                    m.Agenda,
                    m.Notes,
                    m.CreatedAt,
                    m.CreatedByUserId,
                    CreatedByUserName = m.CreatedByUser != null ? m.CreatedByUser.Name : "Bilinmiyor",
                    MeetingNotes = m.MeetingNotes!.Select(mn => new {
                        mn.Id,
                        mn.NoteText,
                        mn.CreatedAt,
                        UserName = mn.User != null ? mn.User.Name : "Bilinmiyor",
                        mn.UserId
                    })
                })
                .ToListAsync();

            return Ok(meetings);
        }

        [HttpPost("{id}/notes")]
        public async Task<IActionResult> AddNote(int id, [FromBody] MeetingNote note)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null) return NotFound("Toplantı bulunamadı.");

            if (note.UserId <= 0) return BadRequest("Geçersiz kullanıcı ID.");

            note.MeetingId = id;
            note.CreatedAt = DateTime.UtcNow;
            _context.MeetingNotes.Add(note);
            await _context.SaveChangesAsync();
            
            // YENİ: Bildirim gönderme
            var noteAuthor = await _context.Users.FindAsync(note.UserId);
            var users = await _context.Users.Where(u => u.IsActive && u.Id != note.UserId).ToListAsync();
            foreach (var user in users)
            {
                _context.Notifications.Add(new Notification {
                    UserId = user.Id,
                    Message = $"{noteAuthor?.Name ?? "Biri"}, '{meeting.Title}' toplantısına yeni bir not ekledi.",
                    Url = "meetings.html"
                });
            }
            await _context.SaveChangesAsync();

            return Ok(note);
        }

        [HttpPut("notes/{noteId}")]
        public async Task<IActionResult> UpdateNote(int noteId, [FromBody] MeetingNote updatedNote)
        {
            var note = await _context.MeetingNotes.FindAsync(noteId);
            if (note == null) return NotFound("Not bulunamadı.");

            note.NoteText = updatedNote.NoteText;
            await _context.SaveChangesAsync();

            return Ok(note);
        }

        [HttpDelete("notes/{noteId}")]
        public async Task<IActionResult> DeleteNote(int noteId)
        {
            var note = await _context.MeetingNotes.FindAsync(noteId);
            if (note == null) return NotFound("Not bulunamadı.");

            _context.MeetingNotes.Remove(note);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddMeeting([FromBody] Meeting meeting)
        {
            if (meeting.CreatedByUserId <= 0) return BadRequest("Geçersiz kullanıcı ID.");
            
            meeting.CreatedAt = DateTime.UtcNow;
            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            // Notify all other active users about the new meeting
            var users = await _context.Users.Where(u => u.IsActive && u.Id != meeting.CreatedByUserId).ToListAsync();
            foreach (var user in users)
            {
                _context.Notifications.Add(new Notification {
                    UserId = user.Id,
                    Message = $"Yeni Toplantı Planlandı: {meeting.Title} ({meeting.MeetingDate:dd.MM.yyyy HH:mm})",
                    Url = "meetings.html"
                });
            }
            await _context.SaveChangesAsync();

            return Ok(meeting);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeeting(int id, [FromBody] Meeting updatedMeeting)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null) return NotFound("Toplantı bulunamadı.");

            meeting.Title = updatedMeeting.Title;
            meeting.MeetingDate = updatedMeeting.MeetingDate;
            meeting.Agenda = updatedMeeting.Agenda;
            meeting.Notes = updatedMeeting.Notes;

            await _context.SaveChangesAsync();
            return Ok(meeting);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null) return NotFound("Toplantı bulunamadı.");

            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Toplantı başarıyla silindi." });
        }
    }
}
