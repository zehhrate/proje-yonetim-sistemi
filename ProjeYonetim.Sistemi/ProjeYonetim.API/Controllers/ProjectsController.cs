using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeYonetim.API.Data;
using ProjeYonetim.API.DTOs; // <-- EKLENEN SATIR
using ProjeYonetim.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Claims;
using Asp.Versioning;
using ProjeYonetim.API.DTOs;



namespace ProjeYonetim.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")] // ROTA GÜNCELLENDİ
    [ApiController]
    [ApiVersion("1.0")] // VERSİYON EKLENDİ
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // EKLENDİ

        public ProjectsController(ApplicationDbContext context) // EKLENDİ


        {
            _context = context; // EKLENDİ
        }
        private int GetUserId()
        {
            // Bu kodda async/await yok, bu doğru.
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
        [HttpGet("test-error")]
        public IActionResult TestError()
        {
            throw new Exception("Bu, bilerek fırlatılan bir test hatasıdır!");
        }

        // GET: api/projects/5
        // GET: api/projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects([FromQuery] PaginationDto paginationDto)
        {
            var userId = GetUserId();

            var query = _context.Projects
                                .Where(p => p.UserId == userId)
                                .AsQueryable();

            var projects = await query
                                  .Skip((paginationDto.PageNumber - 1) * paginationDto.PageSize)
                                  .Take(paginationDto.PageSize)
                                  .ToListAsync();

            return Ok(projects);
        }
        // GET: api/projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var userId = GetUserId();

            // Projeyi hem kendi ID'si hem de kullanıcının ID'si ile arıyoruz.
            var project = await _context.Projects
                                        .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
            {
                // Proje bulunamadı veya kullanıcıya ait değil.
                return NotFound(); // 404 Not Found
            }

            return Ok(project);
        }
        // POST: api/projects
        [HttpPost]
        public async Task<ActionResult<Project>> CreateProject(ProjectCreateDto projectCreateDto)
        {
            var project = new Project
            {
                Name = projectCreateDto.Name,
                Description = projectCreateDto.Description,
                CreatedDate = System.DateTime.UtcNow,
                UserId = GetUserId()
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // `GetProject` metoduna yönlendirme yapıyoruz.
            return Ok(project); // Sadece oluşturulan projeyi 200 OK ile geri dön.
        }
        // PUT: api/projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectCreateDto projectUpdateDto)
        {
            var userId = GetUserId();

            // Önce güncellenecek projeyi bulalım.
            var project = await _context.Projects
    .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId)
    .ConfigureAwait(false); // <--- BURAYA EKLE
            if (project == null)
            {
                // Proje bulunamadı veya bu kullanıcıya ait değilse, yetkisi yok demektir.
                return NotFound();
            }

            // Bulunan projenin alanlarını gelen yeni verilerle güncelle.
            project.Name = projectUpdateDto.Name;
            project.Description = projectUpdateDto.Description;

            // Entity Framework'e bu nesnenin durumunun "Değiştirildi" (Modified) olduğunu bildirebiliriz.
            // Ancak sadece property'leri değiştirmek de çoğu zaman yeterlidir.
            // _context.Entry(project).State = EntityState.Modified;

            await _context.SaveChangesAsync().ConfigureAwait(false); // <--- BURAYA EKLE

            return NoContent(); // 204 No Content -> Başarılı ama geri dönecek bir içerik yok.
        }
        // DELETE: api/projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = GetUserId();

            // Silinecek projeyi bul.
            var project = await _context.Projects
     .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId)
     .ConfigureAwait(false); // <--- BURAYA EKLE

            if (project == null)
            {
                // Proje yok veya kullanıcıya ait değil.
                return NotFound();
            }

            // Projeyi veritabanından sil.
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync().ConfigureAwait(false); // <--- BURAYA EKLE

            return NoContent(); // Başarılı silme işleminden sonra da 204 dönülür.
        }
    }
}