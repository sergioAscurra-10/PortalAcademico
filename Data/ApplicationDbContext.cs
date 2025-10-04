using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;

namespace PortalAcademico.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> // <-- Cambiar aquí
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Restricción: Un usuario no puede matricularse dos veces en el mismo curso.
            builder.Entity<Matricula>()
                .HasIndex(m => new { m.UsuarioId, m.CursoId })
                .IsUnique();

            // Restricción: HorarioInicio < HorarioFin
            builder.Entity<Curso>()
                .ToTable(b => b.HasCheckConstraint("CK_Curso_Horarios", "[HorarioFin] > [HorarioInicio]"));
        }
    }
}
