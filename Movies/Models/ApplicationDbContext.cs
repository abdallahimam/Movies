using Microsoft.EntityFrameworkCore;

namespace Movies.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Entities (Db Tables)
        public DbSet<Genre> Genres { get; set; }

        public DbSet<Movie> Movies { get; set; }

    }
}
