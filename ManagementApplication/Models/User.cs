using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ManagementApplication.Models
{
    public class User
    {
        [Key]
        public int UserNo { get; set; }
        //[Required]
        public string UserId { get; set; }
        //[Required]
        public string? UserName { get; set; }
        //[Required]
        public string Password { get; set; }
        //[Required]
        public string? UserRole { get; set; }
    }
}
