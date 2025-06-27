using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjeYonetim.API.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public ICollection<Project> Projects { get; set; }
    }
}