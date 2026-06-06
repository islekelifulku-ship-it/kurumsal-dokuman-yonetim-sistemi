using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Data;
using Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartmentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetDepartments()
        {
            return Ok(_context.Departments.ToList());
        }

        [HttpPost]
        public IActionResult AddDepartment(Department dept)
        {
            _context.Departments.Add(dept);
            _context.SaveChanges();
            return Ok(dept);
        }
    }
}
