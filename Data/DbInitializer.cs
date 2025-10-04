using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;

namespace PortalAcademico.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            // Aplica migraciones pendientes
            context.Database.Migrate();

            // Seed Roles
            if (!await roleManager.RoleExistsAsync("Coordinador"))
            {
                await roleManager.CreateAsync(new IdentityRole("Coordinador"));
            }
            if (!await roleManager.RoleExistsAsync("Estudiante"))
            {
                await roleManager.CreateAsync(new IdentityRole("Estudiante"));
            }

            // Seed Coordinador User
            if (await userManager.FindByEmailAsync("coordinador@test.com") == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "coordinador@test.com",
                    Email = "coordinador@test.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, "Password123!");
                await userManager.AddToRoleAsync(user, "Coordinador");
            }

            // Seed Cursos
            if (!context.Cursos.Any())
            {
                context.Cursos.AddRange(
                    new Curso { Codigo = "CS101", Nombre = "Introducción a la Programación", Creditos = 4, CupoMaximo = 50, HorarioInicio = new TimeOnly(8, 0), HorarioFin = new TimeOnly(10, 0), Activo = true },
                    new Curso { Codigo = "MA201", Nombre = "Cálculo Avanzado", Creditos = 5, CupoMaximo = 30, HorarioInicio = new TimeOnly(10, 0), HorarioFin = new TimeOnly(12, 0), Activo = true },
                    new Curso { Codigo = "DB301", Nombre = "Bases de Datos", Creditos = 4, CupoMaximo = 40, HorarioInicio = new TimeOnly(14, 0), HorarioFin = new TimeOnly(16, 0), Activo = true },
                    new Curso { Codigo = "IN100", Nombre = "Inglés Técnico", Creditos = 2, CupoMaximo = 25, HorarioInicio = new TimeOnly(16, 0), HorarioFin = new TimeOnly(17, 0), Activo = false } // Curso inactivo de ejemplo
                );
                await context.SaveChangesAsync();
            }
        }
    }
}