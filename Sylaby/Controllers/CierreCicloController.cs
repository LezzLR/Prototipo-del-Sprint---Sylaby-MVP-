using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sylaby.Models;
using System;
using System.Linq;
using System.Security.Claims;

namespace Sylaby.Controllers
{
    [Authorize]
    public class CierreCicloController : Controller
    {
        private readonly SylabyDbContext _context;

        public CierreCicloController(SylabyDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // FLOW: DOCENTE
        // ==========================================

        [HttpGet]
        public IActionResult Docente()
        {
            // Security: Enforce Docente role
            if (!User.IsInRole("Docente"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var email = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Logout", "Auth");
            }

            // Retrieve courses assigned to this Docente and eagerly load their survey if exists
            var courses = _context.Cursos
                .Include(c => c.Encuestas.Where(e => e.DocenteId == user.Id))
                .Where(c => c.DocenteId == user.Id)
                .ToList();

            return View(courses);
        }

        [HttpGet]
        public IActionResult DocenteFormulario(int cursoId)
        {
            // Security: Enforce Docente role
            if (!User.IsInRole("Docente"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var email = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Logout", "Auth");
            }

            var curso = _context.Cursos.FirstOrDefault(c => c.Id == cursoId);
            if (curso == null || curso.DocenteId != user.Id)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            // Load existing survey or prepare a new one
            var encuesta = _context.EncuestasCierreCiclo
                .FirstOrDefault(e => e.CursoId == cursoId && e.DocenteId == user.Id);

            if (encuesta == null)
            {
                encuesta = new EncuestaCierreCiclo
                {
                    CursoId = cursoId,
                    Curso = curso,
                    DocenteId = user.Id,
                    DocenteEmail = user.Email,
                    PorcentajeCumplimiento = 100, // Default to 100%
                    Estado = "Borrador"
                };
            }
            else
            {
                encuesta.Curso = curso; // Bind course navigation property
            }

            return View(encuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DocenteFormulario(int cursoId, int porcentajeCumplimiento, string? temasNoDesarrollados, string? observacionesFinales, string actionType)
        {
            // Security: Enforce Docente role
            if (!User.IsInRole("Docente"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var email = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Logout", "Auth");
            }

            var curso = _context.Cursos.FirstOrDefault(c => c.Id == cursoId && c.DocenteId == user.Id);
            if (curso == null)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var encuesta = _context.EncuestasCierreCiclo
                .FirstOrDefault(e => e.CursoId == cursoId && e.DocenteId == user.Id);

            bool isNew = false;
            if (encuesta == null)
            {
                encuesta = new EncuestaCierreCiclo
                {
                    CursoId = cursoId,
                    Curso = curso,
                    DocenteId = user.Id,
                    DocenteEmail = user.Email,
                    FechaCreacion = DateTime.Now
                };
                isNew = true;
            }
            else
            {
                encuesta.Curso = curso;
            }

            // Lock check: If already submitted, block modifications
            if (!isNew && encuesta.Estado == "Enviada")
            {
                TempData["ErrorMessage"] = "Esta encuesta ya fue enviada definitivamente y no puede modificarse.";
                return RedirectToAction("Docente");
            }

            // Server-Side Validations
            if (porcentajeCumplimiento < 0 || porcentajeCumplimiento > 100)
            {
                ModelState.AddModelError("PorcentajeCumplimiento", "El porcentaje de cumplimiento debe estar entre 0 y 100.");
            }

            if (porcentajeCumplimiento < 100 && string.IsNullOrWhiteSpace(temasNoDesarrollados))
            {
                ModelState.AddModelError("TemasNoDesarrollados", "Los temas no desarrollados son obligatorios si el porcentaje de cumplimiento es menor al 100%.");
            }

            if (!ModelState.IsValid)
            {
                // Re-bind values so they aren't lost in the view
                encuesta.PorcentajeCumplimiento = porcentajeCumplimiento;
                encuesta.TemasNoDesarrollados = temasNoDesarrollados;
                encuesta.ObservacionesFinales = observacionesFinales;
                return View(encuesta);
            }

            // Bind values
            encuesta.PorcentajeCumplimiento = porcentajeCumplimiento;
            encuesta.TemasNoDesarrollados = porcentajeCumplimiento == 100 ? null : temasNoDesarrollados; // Clear topics if 100%
            encuesta.ObservacionesFinales = observacionesFinales;
            encuesta.FechaUltimaModificacion = DateTime.Now;

            if (actionType == "Enviar")
            {
                encuesta.Estado = "Enviada";
                encuesta.FechaEnvioDefinitivo = DateTime.Now;
                TempData["SuccessMessage"] = $"¡Encuesta del curso '{curso.Nombre}' enviada definitivamente con éxito!";
            }
            else
            {
                encuesta.Estado = "Borrador";
                TempData["SuccessMessage"] = $"Borrador del curso '{curso.Nombre}' guardado correctamente.";
            }

            if (isNew)
            {
                _context.EncuestasCierreCiclo.Add(encuesta);
            }
            else
            {
                _context.Entry(encuesta).State = EntityState.Modified;
            }

            _context.SaveChanges();

            return RedirectToAction("Docente");
        }

        // ==========================================
        // FLOW: DIRECTOR
        // ==========================================

        [HttpGet]
        public IActionResult Director(string? selectedDocente, string? selectedCurso, string? selectedEstado)
        {
            // Security: Enforce Director role
            if (!User.IsInRole("Director"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var query = _context.EncuestasCierreCiclo
                .Include(e => e.Curso)
                .Include(e => e.Docente)
                .AsQueryable();

            // Apply Filters
            if (!string.IsNullOrEmpty(selectedDocente))
            {
                query = query.Where(e => e.DocenteEmail == selectedDocente);
            }

            if (!string.IsNullOrEmpty(selectedCurso))
            {
                query = query.Where(e => e.Curso!.Nombre == selectedCurso);
            }

            if (!string.IsNullOrEmpty(selectedEstado))
            {
                query = query.Where(e => e.Estado == selectedEstado);
            }

            var surveys = query.OrderByDescending(e => e.FechaEnvioDefinitivo ?? e.FechaUltimaModificacion).ToList();

            // Populate Filter Option Lists dynamically for a premium interface
            ViewBag.Docentes = _context.Users
                .Where(u => u.Role == "Docente")
                .Select(u => u.Email)
                .Distinct()
                .ToList();

            ViewBag.Cursos = _context.Cursos
                .Select(c => c.Nombre)
                .Distinct()
                .ToList();

            // Hold current selections in ViewBag to maintain filter states
            ViewBag.SelectedDocente = selectedDocente;
            ViewBag.SelectedCurso = selectedCurso;
            ViewBag.SelectedEstado = selectedEstado;

            return View(surveys);
        }

        [HttpGet]
        public IActionResult DirectorDetalles(int id)
        {
            // Security: Enforce Director role
            if (!User.IsInRole("Director"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var encuesta = _context.EncuestasCierreCiclo
                .Include(e => e.Curso)
                .Include(e => e.Docente)
                .FirstOrDefault(e => e.Id == id);

            if (encuesta == null)
            {
                return NotFound();
            }

            return View(encuesta);
        }
    }
}
