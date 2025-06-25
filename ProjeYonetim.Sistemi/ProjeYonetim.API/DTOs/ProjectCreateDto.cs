using System.ComponentModel.DataAnnotations;

namespace ProjeYonetim.API.DTOs
{
    public class ProjectCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}