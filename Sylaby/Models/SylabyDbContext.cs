using Microsoft.EntityFrameworkCore;

namespace Sylaby.Models
{
    public class SylabyDbContext : DbContext
    {
        public SylabyDbContext(DbContextOptions<SylabyDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Email field to be unique in the database
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
