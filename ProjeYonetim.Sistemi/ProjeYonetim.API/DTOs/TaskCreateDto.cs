using System.ComponentModel.DataAnnotations;

namespace ProjeYonetim.API.DTOs
{
    public class TaskCreateDto
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public System.DateTime? DueDate { get; set; }
    }
}