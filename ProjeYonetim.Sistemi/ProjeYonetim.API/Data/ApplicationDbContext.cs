using Microsoft.EntityFrameworkCore;
using ProjeYonetim.API.Models;

namespace ProjeYonetim.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Buraya veritabanında tablo olmasını istediğimiz modelleri ekliyoruz.
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
    }
}