using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    // Solo los usuarios con el rol "Coordinador" pueden acceder.
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public CoordinadorController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Helper para invalidar el caché de cursos.
        private async Task InvalidateCursosCache()
        {
            // La clave debe ser EXACTAMENTE la misma que se usa en CursosController
            await _cache.RemoveAsync("ListaCursosActivos");
        }

        // GET: Coordinador
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cursos.OrderByDescending(c => c.Id).ToListAsync());
        }

        // GET: Coordinador/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }

        // GET: Coordinador/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Coordinador/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
        {
            if (curso.HorarioInicio >= curso.HorarioFin)
            {
                ModelState.AddModelError("HorarioFin", "La hora de fin debe ser posterior a la hora de inicio.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(curso);
                await _context.SaveChangesAsync();
                await InvalidateCursosCache(); // Invalidar caché
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Coordinador/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }
            return View(curso);
        }

        // POST: Coordinador/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
        {
            if (id != curso.Id)
            {
                return NotFound();
            }
            
            if (curso.HorarioInicio >= curso.HorarioFin)
            {
                ModelState.AddModelError("HorarioFin", "La hora de fin debe ser posterior a la hora de inicio.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();
                    await InvalidateCursosCache(); // Invalidar caché
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CursoExists(curso.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Coordinador/Delete/5 (Página de confirmación para desactivar)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }

        // POST: Coordinador/Delete/5 (Acción de desactivación)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso != null)
            {
                curso.Activo = false; // Soft-delete
                _context.Update(curso);
                await _context.SaveChangesAsync();
                await InvalidateCursosCache(); // Invalidar caché
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CursoExists(int id)
        {
            return _context.Cursos.Any(e => e.Id == id);
        }


        // --- SECCIÓN DE GESTIÓN DE MATRÍCULAS ---

        // GET: Coordinador/Matriculas/5
        public async Task<IActionResult> Matriculas(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            ViewBag.CursoNombre = curso.Nombre;
            ViewBag.CursoId = curso.Id;

            var matriculas = await _context.Matriculas
                .Where(m => m.CursoId == id)
                .Include(m => m.Usuario)
                .OrderBy(m => m.FechaRegistro)
                .ToListAsync();

            return View(matriculas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarMatricula(int matriculaId, int cursoId)
        {
            var matricula = await _context.Matriculas.FindAsync(matriculaId);
            if (matricula != null)
            {
                matricula.Estado = EstadoMatricula.Confirmada;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Matriculas", new { id = cursoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarMatricula(int matriculaId, int cursoId)
        {
            var matricula = await _context.Matriculas.FindAsync(matriculaId);
            if (matricula != null)
            {
                matricula.Estado = EstadoMatricula.Cancelada;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Matriculas", new { id = cursoId });
        }
    }
}