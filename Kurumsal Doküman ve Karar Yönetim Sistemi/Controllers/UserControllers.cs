using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Data;
using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = _context.Users
            .Include(u => u.Department)
            .Where(u => u.IsActive)
            .Select(u => new {
                u.Id,
                u.Name,
                u.Email,
                u.Role,
                u.DepartmentId,
                DepartmentName = u.Department != null ? u.Department.Name : null
            })
            .ToList();
        return Ok(users);
    }

    [HttpPost]
    public IActionResult AddUser(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
        return Ok(user);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
    {
        var user = _context.Users.Find(id);
        if (user == null || !user.IsActive) return NotFound("Kullanıcı bulunamadı.");

        user.Name = updatedUser.Name;
        user.Email = updatedUser.Email;
        user.Role = updatedUser.Role;
        user.DepartmentId = updatedUser.DepartmentId;

        _context.SaveChanges();
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound("Kullanıcı bulunamadı.");

        // Soft Delete
        user.IsActive = false;
        
        try
        {
            _context.SaveChanges();
            return Ok(new { message = "Kullanıcı başarıyla pasife alındı." });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, "Silme işlemi sırasında hata oluştu: " + ex.Message);
        }
    }
}
