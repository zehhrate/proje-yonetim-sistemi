using System.ComponentModel.DataAnnotations;

namespace ProjeYonetim.API.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } // Şifreyi asla normal metin olarak tutmayacağız.
    }
}