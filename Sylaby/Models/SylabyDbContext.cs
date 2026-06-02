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
        public DbSet<Silabo> Silabos { get; set; }
        public DbSet<PropuestaMejora> PropuestasMejora { get; set; }
        public DbSet<ObservacionDirector> ObservacionesDirector { get; set; }
        public DbSet<BitacoraAccion> BitacoraAcciones { get; set; }
        public DbSet<ValidacionAcademica> ValidacionesAcademicas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Email field to be unique in the database
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Curso → Docente
            modelBuilder.Entity<Curso>()
                .HasOne(c => c.Docente)
                .WithMany(u => u.Cursos)
                .HasForeignKey(c => c.DocenteId)
                .OnDelete(DeleteBehavior.Cascade);

            // EncuestaCierreCiclo → Curso
            modelBuilder.Entity<EncuestaCierreCiclo>()
                .HasOne(e => e.Curso)
                .WithMany(c => c.Encuestas)
                .HasForeignKey(e => e.CursoId)
                .OnDelete(DeleteBehavior.Cascade);

            // EncuestaCierreCiclo → Docente
            modelBuilder.Entity<EncuestaCierreCiclo>()
                .HasOne(e => e.Docente)
                .WithMany(u => u.Encuestas)
                .HasForeignKey(e => e.DocenteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Silabo → Curso
            modelBuilder.Entity<Silabo>()
                .HasOne(s => s.Curso)
                .WithMany(c => c.Silabos)
                .HasForeignKey(s => s.CursoId)
                .OnDelete(DeleteBehavior.Cascade);

            // PropuestaMejora → Silabo
            modelBuilder.Entity<PropuestaMejora>()
                .HasOne(p => p.Silabo)
                .WithMany(s => s.Propuestas)
                .HasForeignKey(p => p.SilaboId)
                .OnDelete(DeleteBehavior.Cascade);

            // PropuestaMejora → Docente (no cascade to avoid multiple cascade paths)
            modelBuilder.Entity<PropuestaMejora>()
                .HasOne(p => p.Docente)
                .WithMany(u => u.Propuestas)
                .HasForeignKey(p => p.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);

            // ObservacionDirector → Silabo
            modelBuilder.Entity<ObservacionDirector>()
                .HasOne(o => o.Silabo)
                .WithMany(s => s.Observaciones)
                .HasForeignKey(o => o.SilaboId)
                .OnDelete(DeleteBehavior.Cascade);

            // ObservacionDirector → Director (no cascade)
            modelBuilder.Entity<ObservacionDirector>()
                .HasOne(o => o.Director)
                .WithMany(u => u.Observaciones)
                .HasForeignKey(o => o.DirectorId)
                .OnDelete(DeleteBehavior.Restrict);

            // BitacoraAccion → Silabo (optional FK, no cascade chain issues)
            modelBuilder.Entity<BitacoraAccion>()
                .HasOne(b => b.Silabo)
                .WithMany(s => s.Bitacora)
                .HasForeignKey(b => b.SilaboId)
                .OnDelete(DeleteBehavior.SetNull);

            // ValidacionAcademica → Silabo
            modelBuilder.Entity<ValidacionAcademica>()
                .HasOne(v => v.Silabo)
                .WithMany(s => s.ValidacionesAcademicas)
                .HasForeignKey(v => v.SilaboId)
                .OnDelete(DeleteBehavior.Cascade);

            // ValidacionAcademica → Revisor (no cascade)
            modelBuilder.Entity<ValidacionAcademica>()
                .HasOne(v => v.Revisor)
                .WithMany()
                .HasForeignKey(v => v.RevisorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
