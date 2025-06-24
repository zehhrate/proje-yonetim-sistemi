using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ProjeYonetim.API.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Artık biliyoruz ki, komut zaten doğru klasörün içinde çalışıyor.
            // Bu yüzden karmaşık yol hesaplamalarına gerek yok.
            // Sadece mevcut dizini kullanacağız.

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Mevcut klasöre bak
                .AddJsonFile("appsettings.json")            // Bu klasördeki appsettings.json dosyasını bul
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlite(connectionString);

            return new ApplicationDbContext(builder.Options);
        }
    }
}