using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;
using System.Security.Claims;

namespace PortalAcademico.Controllers
{
    // Requerimos que el usuario esté autenticado para CUALQUIER acción en este controlador.
    [Authorize] 
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Inyectamos el DbContext y el UserManager para usarlos en nuestras acciones.
        public MatriculasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Esta es la acción que recibe la petición del botón "Inscribirse"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            // Obtenemos el ID del usuario que está actualmente logueado.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                // Esto no debería pasar gracias al [Authorize], pero es una buena defensa.
                return Challenge(); 
            }

            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null || !curso.Activo)
            {
                TempData["ErrorMessage"] = "El curso seleccionado no es válido o no está activo.";
                return RedirectToAction("Index", "Cursos");
            }
            
            // --- VALIDACIONES SERVER-SIDE ---

            // Validación 1: ¿El usuario ya está matriculado (y no cancelado)?
            var matriculaExistente = await _context.Matriculas
                .AnyAsync(m => m.CursoId == cursoId && m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada);
            
            if (matriculaExistente)
            {
                TempData["ErrorMessage"] = "Ya te encuentras matriculado en este curso.";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }

            // Validación 2: ¿Hay cupo disponible?
            var matriculasConfirmadas = await _context.Matriculas
                .CountAsync(m => m.CursoId == cursoId && m.Estado != EstadoMatricula.Cancelada);

            if (matriculasConfirmadas >= curso.CupoMaximo)
            {
                TempData["ErrorMessage"] = "Lo sentimos, el curso ha alcanzado su cupo máximo.";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }
            
            // Validación 3: ¿El horario se solapa con otro curso ya matriculado?
            // Obtenemos todas las matrículas CONFIRMADAS del usuario para comparar horarios.
            var misOtrasMatriculas = await _context.Matriculas
                .Where(m => m.UsuarioId == userId && m.Estado == EstadoMatricula.Confirmada)
                .Include(m => m.Curso) // Incluimos el Curso para acceder a su horario.
                .ToListAsync();

            foreach (var otraMatricula in misOtrasMatriculas)
            {
                // Lógica de solapamiento de horarios: (InicioA < FinB) y (FinA > InicioB)
                if (curso.HorarioInicio < otraMatricula.Curso.HorarioFin && curso.HorarioFin > otraMatricula.Curso.HorarioInicio)
                {
                    TempData["ErrorMessage"] = $"El horario de este curso se solapa con el de '{otraMatricula.Curso.Nombre}' (de {otraMatricula.Curso.HorarioInicio:hh\\:mm} a {otraMatricula.Curso.HorarioFin:hh\\:mm}).";
                    return RedirectToAction("Details", "Cursos", new { id = cursoId });
                }
            }
            
            // --- SI TODAS LAS VALIDACIONES PASAN, CREAMOS LA MATRÍCULA ---
            var nuevaMatricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = userId,
                FechaRegistro = DateTime.UtcNow,
                Estado = EstadoMatricula.Pendiente // El estado inicial es Pendiente
            };

            _context.Matriculas.Add(nuevaMatricula);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"¡Pre-inscripción exitosa en '{curso.Nombre}'! Tu solicitud está ahora pendiente de confirmación por un coordinador.";
            return RedirectToAction("Details", "Cursos", new { id = cursoId });
        }
    }
}