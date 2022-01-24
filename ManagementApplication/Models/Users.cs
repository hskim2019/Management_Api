using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ManagementApplication.Models
{
    public class User
    {
        [Key]
        public int UserIdx { get; set; }
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public string Password { get; set; }
        public string? UserRole { get; set; }
    }
}
