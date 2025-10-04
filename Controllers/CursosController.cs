using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data; // Asumiendo que tu DbContext está en esta carpeta

namespace PortalAcademico.Controllers
{
    // Inherit from the base Controller class
    public class CursosController : Controller
    {
        // Declare and inject the DbContext
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? minCreditos, int? maxCreditos, TimeOnly? horarioInicio)
        {
            // The ViewData property is inherited from Controller, no need to declare it.
            ViewData["CurrentFilter"] = searchString;
            ViewData["MinCreditos"] = minCreditos;
            ViewData["MaxCreditos"] = maxCreditos;
            ViewData["HorarioInicio"] = horarioInicio?.ToString("HH:mm");

            var cursos = from c in _context.Cursos
                         where c.Activo
                         select c;

            if (!String.IsNullOrEmpty(searchString))
            {
                cursos = cursos.Where(s => s.Nombre.Contains(searchString) || s.Codigo.Contains(searchString));
            }

            if (minCreditos.HasValue)
            {
                if (minCreditos < 0) minCreditos = 0;
                cursos = cursos.Where(c => c.Creditos >= minCreditos);
            }

            if (maxCreditos.HasValue)
            {
                if (maxCreditos < 0) maxCreditos = 0;
                cursos = cursos.Where(c => c.Creditos <= maxCreditos);
            }

            if (horarioInicio.HasValue)
            {
                // This will only work if HorarioInicio in your model is TimeOnly.
                // It might not be translatable to some database providers.
                cursos = cursos.Where(c => c.HorarioInicio >= horarioInicio.Value);
            }

            // Using AsNoTracking() is good for performance on read-only queries
            return View(await cursos.AsNoTracking().ToListAsync());
        } 
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) { return NotFound(); }
            var curso = await _context.Cursos.FirstOrDefaultAsync(m => m.Id == id);
            if (curso == null) { return NotFound(); }

            // --- LÓGICA DE SESIÓN ---
            HttpContext.Session.SetString("LastVisitedCourseId", curso.Id.ToString());
            HttpContext.Session.SetString("LastVisitedCourseName", curso.Nombre);
            // -----------------------

            return View(curso);
        }
    }
}