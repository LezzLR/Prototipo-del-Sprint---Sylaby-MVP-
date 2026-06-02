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
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<EncuestaCierreCiclo> EncuestasCierreCiclo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Email field to be unique in the database
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure relations
            modelBuilder.Entity<Curso>()
                .HasOne(c => c.Docente)
                .WithMany(u => u.Cursos)
                .HasForeignKey(c => c.DocenteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EncuestaCierreCiclo>()
                .HasOne(e => e.Curso)
                .WithMany(c => c.Encuestas)
                .HasForeignKey(e => e.CursoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EncuestaCierreCiclo>()
                .HasOne(e => e.Docente)
                .WithMany(u => u.Encuestas)
                .HasForeignKey(e => e.DocenteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
