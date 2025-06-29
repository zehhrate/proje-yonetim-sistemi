using Asp.Versioning; // VERSİYONLAMA İÇİN EKLENDİ
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ProjeYonetim.API.Data;
using ProjeYonetim.API.DTOs;
using ProjeYonetim.API.Models;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace ProjeYonetim.API.Controllers
{
    // =================================================================
    //               VERSİYONLAMA ATTRIBUTE'LERİ EKLENDİ
    // =================================================================
    [Route("api/v{version:apiVersion}/[controller]")] // Rota güncellendi
    [ApiController]
    [ApiVersion("1.0")] // Bu controller'ın v1.0'a ait olduğunu belirt
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, IConfiguration config, ILogger<AuthController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            _logger.LogInformation("Yeni kullanıcı kayıt isteği alındı: {Email}", userRegisterDto.Email);
            if (await _context.Users.AnyAsync(u => u.Email == userRegisterDto.Email))
            {
                _logger.LogWarning("Kayıt denemesi başarısız: E-posta zaten kullanılıyor - {Email}", userRegisterDto.Email);
                return BadRequest("Bu e-posta adresi zaten kullanılıyor.");
            }
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password);
            var user = new AppUser
            {
                FullName = userRegisterDto.FullName,
                Email = userRegisterDto.Email,
                PasswordHash = passwordHash,
                Role = "User"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Kullanıcı başarıyla kaydedildi: Id = {UserId}, Email = {Email}", user.Id, user.Email);
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            _logger.LogInformation("Kullanıcı giriş denemesi: {Email}", userLoginDto.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Başarısız giriş denemesi: {Email}", userLoginDto.Email);
                return Unauthorized("E-posta veya şifre hatalı.");
            }
            var token = CreateJwtToken(user);
            _logger.LogInformation("Kullanıcı başarıyla giriş yaptı: {Email}", user.Email);
            return Ok(new { token });
        }

        [HttpGet("all-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("Admin yetkili bir kullanıcı, tüm kullanıcıları listeleme isteği gönderdi.");
            var users = await _context.Users.Select(u => new { u.Id, u.FullName, u.Email, u.Role }).ToListAsync();
            return Ok(users);
        }

        [HttpGet("protected")]
        [Authorize]
        public IActionResult ProtectedEndpoint()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Korumalı endpoint'e erişildi. Erişen kullanıcı ID: {UserId}", userId);
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            return Ok($"Merhaba {userEmail}! Bu korumalı bir alandır. Kullanıcı ID'niz: {userId}");
        }

        private string CreateJwtToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            if (!string.IsNullOrEmpty(user.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = System.DateTime.Now.AddDays(1),
                SigningCredentials = creds,
                Issuer = _config.GetSection("Jwt:Issuer").Value,
                Audience = _config.GetSection("Jwt:Audience").Value
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}