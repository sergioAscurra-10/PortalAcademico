using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data; // Asumiendo que tu DbContext está en esta carpeta
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    // Inherit from the base Controller class
    public class CursosController : Controller
    {
        // Declare and inject the DbContext
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        public CursosController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IActionResult> Index(string searchString, int? minCreditos, int? maxCreditos, TimeOnly? horarioInicio)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["MinCreditos"] = minCreditos;
            ViewData["MaxCreditos"] = maxCreditos;
            ViewData["HorarioInicio"] = horarioInicio?.ToString("HH:mm");

            bool hasFilters = !string.IsNullOrEmpty(searchString) || minCreditos.HasValue || maxCreditos.HasValue || horarioInicio.HasValue;
            
            if (hasFilters)
            {
                var cursos = from c in _context.Cursos
                             where c.Activo
                             select c;

                if (!string.IsNullOrEmpty(searchString))
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
                    cursos = cursos.Where(c => c.HorarioInicio >= horarioInicio.Value);
                }

                var cursosFiltrados = await cursos.AsNoTracking().ToListAsync();
                return View(cursosFiltrados);
            }
            
            const string cacheKey = "ListaCursosActivos";
            List<Curso> cursosCache;

            var cachedData = await _cache.GetAsync(cacheKey);
            if (cachedData != null)
            {
                cursosCache = JsonSerializer.Deserialize<List<Curso>>(System.Text.Encoding.UTF8.GetString(cachedData));
            }
            else
            {
                cursosCache = await _context.Cursos.Where(c => c.Activo).AsNoTracking().ToListAsync();
                var dataToCache = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cursosCache));
                var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                await _cache.SetAsync(cacheKey, dataToCache, cacheOptions);
            }

            return View(cursosCache);
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