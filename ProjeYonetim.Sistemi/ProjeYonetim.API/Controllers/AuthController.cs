using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeYonetim.API.Data;
using BCrypt.Net;
using ProjeYonetim.API.DTOs;
using ProjeYonetim.API.Models;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration; // IConfiguration için
using Microsoft.IdentityModel.Tokens;     // SymmetricSecurityKey için
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization; // Bu using'i dosyanın en üstüne ekle

namespace ProjeYonetim.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config; // Yeni eklendi

        public AuthController(ApplicationDbContext context, IConfiguration config) // Yeni parametre eklendi
        {
            _context = context;
            _config = config; // Yeni eklendi
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
        // POST: api/auth/login
        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
            {
                return Unauthorized("E-posta veya şifre hatalı.");
            }

            // Giriş başarılı! JWT'yi oluştur.
            var token = CreateJwtToken(user);

            // Token'ı kullanıcıya geri dön.
            return Ok(new { token = token });
        }
      

// ... AuthController sınıfının içinde ...

[HttpGet("protected")]
    [Authorize] // BU KISIM ÇOK ÖNEMLİ!
    public IActionResult ProtectedEndpoint()
    {
        // Token'dan gelen bilgileri okuyabiliriz.
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Ok($"Merhaba {userEmail}! Bu korumalı bir alandır. Kullanıcı ID'niz: {userId}");
    }
    private string CreateJwtToken(User user)
        {
            // 1. Claim'leri oluştur
            // Claim'ler, token'ın içinde taşıyacağımız bilgilerdir (kullanıcı ID, e-posta, roller vb.)
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kullanıcının benzersiz ID'si
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.FullName)
        // Buraya rol gibi başka bilgiler de eklenebilir: new Claim(ClaimTypes.Role, "Admin")
    };

            // 2. appsettings.json'dan gizli anahtarı al
            // Bu işlemi yapabilmek için IConfiguration servisine ihtiyacımız var.
            // Onu constructor'a ekleyeceğiz.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config.GetSection("Jwt:Key").Value));

            // 3. Anahtar ile kullanılacak şifreleme algoritmasını belirle
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // 4. Token'ın tanımını (descriptor) oluştur
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1), // Token 1 gün geçerli olacak
                SigningCredentials = creds,
                Issuer = _config.GetSection("Jwt:Issuer").Value,
                Audience = _config.GetSection("Jwt:Audience").Value
            };

            // 5. Token'ı oluştur
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // 6. Token'ı string formatında geri dön
            return tokenHandler.WriteToken(token);
        }
    }
}
