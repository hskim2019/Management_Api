using Microsoft.EntityFrameworkCore;

namespace ManagementApplication.Models
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : 
            base(options)
        {

        }

        public DbSet<User> users { get; set; } = null!;

    }
}
