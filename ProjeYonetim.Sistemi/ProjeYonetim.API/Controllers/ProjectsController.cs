using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjeYonetim.API.Data; // EKLENDİ

namespace ProjeYonetim.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // EKLENDİ

        public ProjectsController(ApplicationDbContext context) // EKLENDİ
        {
            _context = context; // EKLENDİ
        }

        [HttpGet]
        public IActionResult GetProjects()
        {
            return Ok("Projects endpoint'i DbContext ile çalışıyor.");
        }
    }
}