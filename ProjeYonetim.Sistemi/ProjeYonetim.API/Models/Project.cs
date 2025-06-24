using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjeYonetim.API.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = ""; // Başlangıç değeri ata

        public string Description { get; set; } = ""; // Başlangıç değeri ata

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }
    }
}