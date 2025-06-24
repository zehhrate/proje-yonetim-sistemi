using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjeYonetim.API.Models
{
    public class AppUser // <-- İSİM DEĞİŞTİ
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public ICollection<Project> Projects { get; set; }
    }
}