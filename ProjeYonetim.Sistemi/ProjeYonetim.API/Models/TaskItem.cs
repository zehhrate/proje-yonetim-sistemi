using System.ComponentModel.DataAnnotations;

namespace ProjeYonetim.API.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime DueDate { get; set; } // Son teslim tarihi
    }
}