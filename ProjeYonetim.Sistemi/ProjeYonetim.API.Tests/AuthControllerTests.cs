using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjeYonetim.API.Controllers;
using ProjeYonetim.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using ProjeYonetim.API.Data;
using Microsoft.EntityFrameworkCore;

namespace ProjeYonetim.API.Tests
{
    public class AuthControllerTests
    {
        // Test metodları [Fact] attribute'ü ile işaretlenir.
        [Fact]
        public void CreateJwtToken_KullaniciAdminIse_TokenIcindeAdminRoluOlmali()
        {
            // --- ARRANGE (HAZIRLIK) ---

            // 1. Sahte IConfiguration'ı hazırla
            // appsettings.json'daki Jwt ayarlarını taklit ediyoruz.
            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:Key", "BU_COK_GIZLI_VE_UZUN_BIR_ANAHTAR_OLMALI_EN_AZ_16_KARAKTER_YOKSA_CALISMAZ"},
                {"Jwt:Issuer", "test.com"},
                {"Jwt:Audience", "test.com"}
            };
            IConfiguration mockConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // 2. Sahte ILogger'ı hazırla
            var mockLogger = new Mock<ILogger<AuthController>>();

            // 3. Sahte DbContext'i hazırla (Buna ihtiyacımız yok çünkü metot DbContext kullanmıyor)
            // Ama constructor istediği için boş bir mock oluşturuyoruz.
            var mockDbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

            // 4. Test edilecek AuthController'ın bir örneğini oluştur
            var authController = new AuthController(mockDbContext.Object, mockConfiguration, mockLogger.Object);

            // 5. Test için sahte bir kullanıcı oluştur
            var adminUser = new AppUser { Id = 1, Email = "admin@test.com", FullName = "Test Admin", Role = "Admin" };

            // --- ACT (EYLEM) ---

            // Test edeceğimiz CreateJwtToken metodunu çağırmak için Reflection kullanıyoruz,
            // çünkü metot 'private' olarak tanımlı.
            var privateMethod = typeof(AuthController).GetMethod("CreateJwtToken",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var tokenString = (string)privateMethod.Invoke(authController, new object[] { adminUser });

            // --- ASSERT (DOĞRULAMA) ---

            // 1. Token'ın boş olmadığını doğrula
            Assert.False(string.IsNullOrEmpty(tokenString));

            // 2. Token'ın içini aç ve rol claim'ini bul
            var handler = new JwtSecurityTokenHandler();
            var decodedToken = handler.ReadJwtToken(tokenString);
            var roleClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            // 3. Rol claim'inin var olduğunu ve değerinin "Admin" olduğunu doğrula
            Assert.NotNull(roleClaim);
            Assert.Equal("Admin", roleClaim.Value);

            Xunit.Assert.True(true, "Test başarılı!"); // Konsolda görmek için
        }
    }
}