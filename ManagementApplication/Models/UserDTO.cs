using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ManagementApplication.Models
{
    public class UserDTO
    {
        [Key]
        public int UserIdx { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }

        [JsonIgnore] //The [JsonIgnore] attribute prevents the password property from being serialized and returned in api responses
        public string Password { get; set; }

    }
}
