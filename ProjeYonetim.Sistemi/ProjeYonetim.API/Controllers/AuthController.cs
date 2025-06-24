using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeYonetim.API.Data;
using BCrypt.Net;
using ProjeYonetim.API.DTOs;
using ProjeYonetim.API.Models;
using System.Threading.Tasks;

namespace ProjeYonetim.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/auth/register
        // POST: api/auth/register
        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            // 1. E-postanın zaten var olup olmadığını kontrol et
            if (await _context.Users.AnyAsync(u => u.Email == userRegisterDto.Email))
            {
                return BadRequest("Bu e-posta adresi zaten kullanılıyor.");
            }

            // 2. Şifreyi hash'le
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password);

            // 3. Yeni kullanıcı nesnesini oluştur
            var user = new User
            {
                FullName = userRegisterDto.FullName,
                Email = userRegisterDto.Email,
                PasswordHash = passwordHash // Hash'lenmiş şifreyi kaydet
            };

            // 4. Veritabanına ekle ve kaydet
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 5. Başarı durumunu bildir
            return StatusCode(201); // 201 Created standart bir cevaptır.
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public IActionResult Login(UserLoginDto userLoginDto)
        {
            // TODO: Kullanıcı giriş ve JWT oluşturma mantığını buraya yazacağız.
            return Ok("Kullanıcı giriş isteği alındı.");
        }
    }
}