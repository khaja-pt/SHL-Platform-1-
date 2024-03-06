using Microsoft.EntityFrameworkCore;
using SHL_Platform.Models;

namespace SHL_Platform.Data
{
    public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
    {
        public DbSet<Admin> Admins { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Survey> Surveys { get; set; }
    }
}
