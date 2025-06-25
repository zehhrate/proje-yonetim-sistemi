using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjeYonetim.API.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public System.DateTime CreatedDate { get; set; }

        // İlişki Alanları
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }
    }
}