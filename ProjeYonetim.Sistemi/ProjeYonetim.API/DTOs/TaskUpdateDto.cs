using System.ComponentModel.DataAnnotations;

namespace ProjeYonetim.API.DTOs
{
    public class TaskUpdateDto
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public System.DateTime? DueDate { get; set; }
    }
}