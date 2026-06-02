using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sylaby.Models;
using System.Security.Claims;

namespace Sylaby.Controllers
{
    [Authorize]
    public class SilabosController : Controller
    {
        private readonly SylabyDbContext _context;

        public SilabosController(SylabyDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // DOCENTE VIEWS
        // ─────────────────────────────────────────────────────────────────────

        [Authorize(Roles = "Docente")]
        public async Task<IActionResult> DocenteIndex()
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var docente = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (docente == null) return RedirectToAction("Login", "Auth");

            var cursos = await _context.Cursos
                .Where(c => c.DocenteId == docente.Id)
                .Include(c => c.Silabos)
                .ToListAsync();

            return View(cursos);
        }

        [Authorize(Roles = "Docente")]
        public async Task<IActionResult> Editor(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var docente = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (docente == null) return Unauthorized();

            var silabo = await _context.Silabos
                .Include(s => s.Curso)
                .Include(s => s.Propuestas)
                .Include(s => s.Observaciones)
                .FirstOrDefaultAsync(s => s.Id == id && s.Curso!.DocenteId == docente.Id);

            if (silabo == null) return NotFound();

            return View(silabo);
        }

        [HttpPost]
        [Authorize(Roles = "Docente")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(Silabo model)
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var docente = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (docente == null) return Unauthorized();

            var silabo = await _context.Silabos
                .Include(s => s.Curso)
                .FirstOrDefaultAsync(s => s.Id == model.Id && s.Curso!.DocenteId == docente.Id);

            if (silabo == null) return NotFound();

            // Prevent editing if locked states
            var estadosBloqueados = new[] { "En revisión", "Aprobado por Director", "En validación académica", "Aprobado Final" };
            if (estadosBloqueados.Contains(silabo.Estado))
            {
                TempData["Error"] = "El sílabo no puede editarse en su estado actual.";
                return RedirectToAction("Editor", new { id = silabo.Id });
            }

            // Detect modified sections to remove their observations
            var seccionesModificadas = new List<string>();

            if (silabo.EscuelaProfesional != model.EscuelaProfesional ||
                silabo.CicloAcademico != model.CicloAcademico ||
                silabo.Creditos != model.Creditos ||
                silabo.HorasSemanales != model.HorasSemanales ||
                silabo.DocenteResponsable != model.DocenteResponsable)
            {
                seccionesModificadas.Add("Datos Generales");
            }

            if (silabo.Sumilla != model.Sumilla) seccionesModificadas.Add("Sumilla");
            if (silabo.Competencias != model.Competencias) seccionesModificadas.Add("Competencias");
            if (silabo.Capacidades != model.Capacidades) seccionesModificadas.Add("Capacidades");
            if (silabo.ProgramacionContenidos != model.ProgramacionContenidos) seccionesModificadas.Add("Programación de Contenidos");
            if (silabo.EstrategiasDiddacticas != model.EstrategiasDiddacticas) seccionesModificadas.Add("Estrategias Didácticas");
            if (silabo.SistemaEvaluacion != model.SistemaEvaluacion) seccionesModificadas.Add("Sistema de Evaluación");
            if (silabo.Bibliografia != model.Bibliografia) seccionesModificadas.Add("Bibliografía");

            // Update content fields
            silabo.EscuelaProfesional = model.EscuelaProfesional;
            silabo.CicloAcademico = model.CicloAcademico;
            silabo.Creditos = model.Creditos;
            silabo.HorasSemanales = model.HorasSemanales;
            silabo.DocenteResponsable = model.DocenteResponsable;
            silabo.Sumilla = model.Sumilla;
            silabo.Competencias = model.Competencias;
            silabo.Capacidades = model.Capacidades;
            silabo.ProgramacionContenidos = model.ProgramacionContenidos;
            silabo.EstrategiasDiddacticas = model.EstrategiasDiddacticas;
            silabo.SistemaEvaluacion = model.SistemaEvaluacion;
            silabo.Bibliografia = model.Bibliografia;
            silabo.FechaModificacion = DateTime.Now;
            silabo.ModificadoPor = email!;

            // Remove observations for modified sections
            if (seccionesModificadas.Any())
            {
                var obsAEliminar = await _context.ObservacionesDirector
                    .Where(o => o.SilaboId == silabo.Id && seccionesModificadas.Contains(o.SeccionObservada))
                    .ToListAsync();

                if (obsAEliminar.Any())
                {
                    _context.ObservacionesDirector.RemoveRange(obsAEliminar);
                }
            }

            // Log action
            _context.BitacoraAcciones.Add(new BitacoraAccion
            {
                SilaboId = silabo.Id,
                CursoId = silabo.CursoId,
                Usuario = email!,
                Rol = "Docente",
                Accion = "Guardado de borrador" + (seccionesModificadas.Any() ? $" (cambios en: {string.Join(", ", seccionesModificadas)})" : ""),
                Fecha = DateTime.Now,
                Hora = DateTime.Now.ToString("HH:mm")
            });

            await _context.SaveChangesAsync();
            TempData["Exito"] = "Borrador guardado correctamente.";
            return RedirectToAction("Editor", new { id = silabo.Id });
        }

        [HttpPost]
        [Authorize(Roles = "Docente")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarPropuesta(int silaboId, string seccionAfectada, string propuestaActualizacion, string justificacion)
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var docente = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (docente == null) return Unauthorized();

            var silabo = await _context.Silabos
                .Include(s => s.Curso)
                .FirstOrDefaultAsync(s => s.Id == silaboId && s.Curso!.DocenteId == docente.Id);

            if (silabo == null) return NotFound();

            if (string.IsNullOrWhiteSpace(seccionAfectada) || string.IsNullOrWhiteSpace(propuestaActualizacion) || string.IsNullOrWhiteSpace(justificacion))
            {
                TempData["Error"] = "Todos los campos de la propuesta son obligatorios.";
                return RedirectToAction("Editor", new { id = silaboId });
            }

            _context.PropuestasMejora.Add(new PropuestaMejora
            {
                SilaboId = silaboId,
                DocenteId = docente.Id,
                SeccionAfectada = seccionAfectada,
                PropuestaActualizacion = propuestaActualizacion,
                Justificacion = justificacion,
                Fecha = DateTime.Now,
                Hora = DateTime.Now.ToString("HH:mm"),
                Autor = email!
            });

            _context.BitacoraAcciones.Add(new BitacoraAccion
            {
                SilaboId = silaboId,
                CursoId = silabo.CursoId,
                Usuario = email!,
                Rol = "Docente",
                Accion = $"Propuesta registrada: {seccionAfectada}",
                Fecha = DateTime.Now,
                Hora = DateTime.Now.ToString("HH:mm")
            });

            await _context.SaveChangesAsync();
            TempData["Exito"] = "Propuesta agregada correctamente.";
            return RedirectToAction("Editor", new { id = silaboId });
        }

        [HttpPost]
        [Authorize(Roles = "Docente")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarRevision(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var docente = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (docente == null) return Unauthorized();

            var silabo = await _context.Silabos
                .Include(s => s.Curso)
                .Include(s => s.Propuestas)
                .FirstOrDefaultAsync(s => s.Id == id && s.Curso!.DocenteId == docente.Id);

            if (silabo == null) return NotFound();

            // Validations
            var errores = new List<string>();
            if (string.IsNullOrWhiteSpace(silabo.Sumilla)) errores.Add("La Sumilla es obligatoria.");
            if (string.IsNullOrWhiteSpace(silabo.Competencias)) errores.Add("Las Competencias son obligatorias.");
            if (string.IsNullOrWhiteSpace(silabo.Capacidades)) errores.Add("Las Capacidades son obligatorias.");
            if (string.IsNullOrWhiteSpace(silabo.ProgramacionContenidos)) errores.Add("La Programación de Contenidos es obligatoria.");

            if (errores.Any())
            {
                TempData["Error"] = string.Join(" | ", errores);
                return RedirectToAction("Editor", new { id });
            }

            silabo.Estado = "En revisión";
            silabo.FechaEnvio = DateTime.Now;
            silabo.FechaModificacion = DateTime.Now;
            silabo.ModificadoPor = email!;
            silabo.PermiteNuevasPropuestas = false;

            _context.BitacoraAcciones.Add(new BitacoraAccion
            {
                SilaboId = silabo.Id,
                CursoId = silabo.CursoId,
                Usuario = email!,
                Rol = "Docente",
                Accion = "Envío a revisión",
                Fecha = DateTime.Now,
                Hora = DateTime.Now.ToString("HH:mm")
            });

            await _context.SaveChangesAsync();
            TempData["Exito"] = "Sílabo enviado a revisión correctamente.";
            return RedirectToAction("DocenteIndex");
        }

        // ─────────────────────────────────────────────────────────────────────
        // DIRECTOR VIEWS
        // ─────────────────────────────────────────────────────────────────────

        [Authorize(Roles = "Director")]
        public async Task<IActionResult> DirectorIndex(string? filtroCurso, string? filtroDocente, string? filtroEstado)
        {
            var query = _context.Silabos
                .Include(s => s.Curso)
                    .ThenInclude(c => c!.Docente)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filtroCurso))
                query = query.Where(s => s.Curso!.Nombre.Contains(filtroCurso));

            if (!string.IsNullOrEmpty(filtroDocente))
                query = query.Where(s => s.DocenteResponsable.Contains(filtroDocente));

            if (!string.IsNullOrEmpty(filtroEstado))
                query = query.Where(s => s.Estado == filtroEstado);

            ViewBag.FiltroCurso = filtroCurso;
            ViewBag.FiltroDocente = filtroDocente;
            ViewBag.FiltroEstado = filtroEstado;

            var silabos = await query.OrderByDescending(s => s.FechaModificacion).ToListAsync();
            return View(silabos);
        }

        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Detalle(int id)
        {
            var silabo = await _context.Silabos
                .Include(s => s.Curso)
                    .ThenInclude(c => c!.Docente)
                .Include(s => s.Propuestas)
                .Include(s => s.Observaciones)
                .Include(s => s.Bitacora)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (silabo == null) return NotFound();
            return View(silabo);
        }

        [HttpPost]
        [Authorize(Roles = "Director")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id, string? comentarioGeneral,
            string? obsGenerales, string? obsSumilla, string? obsCompetencias,
            string? obsCapacidades, string? obsContenidos, string? obsEstrategias,
            string? obsEvaluacion, string? obsBibliografia)
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var director = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (director == null) return Unauthorized();

            var silabo = await _context.Silabos.FindAsync(id);
            if (silabo == null) return NotFound();

            // Save any observations
            await GuardarObservaciones(id, director, comentarioGeneral, obsGenerales, obsSumilla,
                obsCompetencias, obsCapacidades, obsContenidos, obsEstrategias, obsEvaluacion, obsBibliografia);

            silabo.Estado = "Aprobado por Director";
            silabo.FechaAprobacion = DateTime.Now;
            silabo.AprobadoPor = email!;
            silabo.FechaModificacion = DateTime.Now;
            silabo.PermiteNuevasPropuestas = false;

            _context.BitacoraAcciones.Add(new BitacoraAccion
            {
                SilaboId = id,
                CursoId = silabo.CursoId,
                Usuario = email!,
                Rol = "Director",
                Accion = "Sílabo aprobado por Director — pendiente de validación académica",
                Fecha = DateTime.Now,
                Hora = DateTime.Now.ToString("HH:mm")
            });

            await _context.SaveChangesAsync();
            TempData["Exito"] = "Sílabo aprobado. Pasa a validación del Departamento Académico.";
            return RedirectToAction("DirectorIndex");
        }

        [HttpPost]
        [Authorize(Roles = "Director")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id, string? comentarioGeneral,
            string? obsGenerales, string? obsSumilla, string? obsCompetencias,
            string? obsCapacidades, string? obsContenidos, string? obsEstrategias,
            string? obsEvaluacion, string? obsBibliografia)
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var director = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (director == null) return Unauthorized();

            var silabo = await _context.Silabos.FindAsync(id);
            if (silabo == null) return NotFound();

            // Save observations
            await GuardarObservaciones(id, director, comentarioGeneral, obsGenerales, obsSumilla,
                obsCompetencias, obsCapacidades, obsContenidos, obsEstrategias, obsEvaluacion, obsBibliografia);

            silabo.Estado = "Por corregir";
            silabo.FechaRechazo = DateTime.Now;
            silabo.RechazadoPor = email!;
            silabo.FechaModificacion = DateTime.Now;

            _context.BitacoraAcciones.Add(new BitacoraAccion
            {
                SilaboId = id,
                CursoId = silabo.CursoId,
                Usuario = email!,
                Rol = "Director",
                Accion = "Sílabo rechazado — devuelto para corrección",
                Fecha = DateTime.Now,
                Hora = DateTime.Now.ToString("HH:mm")
            });

            await _context.SaveChangesAsync();
            TempData["Exito"] = "Sílabo rechazado. El docente podrá ver las observaciones y corregirlo.";
            return RedirectToAction("DirectorIndex");
        }

        [HttpPost]
        [Authorize(Roles = "Director")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePermisoEdicion(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var silabo = await _context.Silabos.FindAsync(id);
            if (silabo == null) return NotFound();

            silabo.PermiteNuevasPropuestas = true;
            silabo.Estado = "En edición";
            silabo.FechaModificacion = DateTime.Now;

            _context.BitacoraAcciones.Add(new BitacoraAccion
            {
                SilaboId = id,
                CursoId = silabo.CursoId,
                Usuario = email!,
                Rol = "Director",
                Accion = "Reapertura de edición: El sílabo regresa al estado En edición",
                Fecha = DateTime.Now,
                Hora = DateTime.Now.ToString("HH:mm")
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("Detalle", new { id });
        }

        // ────────────────────────────────────────────────────────────────────────
        // REVISOR ACADÉMICO VIEWS
        // ────────────────────────────────────────────────────────────────────────

        [Authorize(Roles = "RevisorAcademico")]
        public async Task<IActionResult> ValidacionIndex(string? filtroCurso, string? filtroDocente, string? filtroEstado)
        {
            var query = _context.Silabos
                .Include(s => s.Curso)
                    .ThenInclude(c => c!.Docente)
                .Where(s => s.Estado == "Aprobado por Director"
                         || s.Estado == "En validación académica"
                         || s.Estado == "Observado"
                         || s.Estado == "Aprobado Final")
                .AsQueryable();

            if (!string.IsNullOrEmpty(filtroCurso))
                query = query.Where(s => s.Curso!.Nombre.Contains(filtroCurso));

            if (!string.IsNullOrEmpty(filtroDocente))
                query = query.Where(s => s.DocenteResponsable.Contains(filtroDocente));

            if (!string.IsNullOrEmpty(filtroEstado))
                query = query.Where(s => s.Estado == filtroEstado);

            ViewBag.FiltroCurso = filtroCurso;
            ViewBag.FiltroDocente = filtroDocente;
            ViewBag.FiltroEstado = filtroEstado;

            var silabos = await query.OrderByDescending(s => s.FechaAprobacion).ToListAsync();
            return View(silabos);
        }

        [Authorize(Roles = "RevisorAcademico")]
        public async Task<IActionResult> DetalleValidacion(int id)
        {
            var silabo = await _context.Silabos
                .Include(s => s.Curso)
                    .ThenInclude(c => c!.Docente)
                .Include(s => s.Propuestas)
                .Include(s => s.Observaciones)
                .Include(s => s.Bitacora)
                .Include(s => s.ValidacionesAcademicas)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (silabo == null) return NotFound();

            // Mark as "En validación académica" once the revisor opens it
            if (silabo.Estado == "Aprobado por Director")
            {
                var emailRev = User.FindFirstValue(ClaimTypes.Name);
                silabo.Estado = "En validación académica";
                silabo.FechaModificacion = DateTime.Now;

                _context.BitacoraAcciones.Add(new BitacoraAccion
                {
                    SilaboId = silabo.Id,
                    CursoId = silabo.CursoId,
                    Usuario = emailRev!,
                    Rol = "RevisorAcademico",
                    Accion = "Inicio de revisión académica",
                    Fecha = DateTime.Now,
                    Hora = DateTime.Now.ToString("HH:mm")
                });

                await _context.SaveChangesAsync();
            }

            return View(silabo);
        }

        [HttpPost]
        [Authorize(Roles = "RevisorAcademico")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprobarFinal(int id,
            bool datosGeneralesConforme, string? datosGeneralesComentario,
            bool sumillaConforme, string? sumillaComentario,
            bool competenciasConforme, string? competenciasComentario,
            bool capacidadesConforme, string? capacidadesComentario,
            bool contenidosConforme, string? contenidosComentario,
            bool estrategiasConforme, string? estrategiasComentario,
            bool evaluacionConforme, string? evaluacionComentario,
            bool bibliografiaConforme, string? bibliografiaComentario,
            string? observacionGeneral)
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var revisor = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (revisor == null) return Unauthorized();

            var silabo = await _context.Silabos.FindAsync(id);
            if (silabo == null) return NotFound();

            // Validate: No Conforme items must have a comment
            var errores = new List<string>();
            if (!datosGeneralesConforme && string.IsNullOrWhiteSpace(datosGeneralesComentario)) errores.Add("Datos Generales");
            if (!sumillaConforme && string.IsNullOrWhiteSpace(sumillaComentario)) errores.Add("Sumilla");
            if (!competenciasConforme && string.IsNullOrWhiteSpace(competenciasComentario)) errores.Add("Competencias");
            if (!capacidadesConforme && string.IsNullOrWhiteSpace(capacidadesComentario)) errores.Add("Capacidades");
            if (!contenidosConforme && string.IsNullOrWhiteSpace(contenidosComentario)) errores.Add("Contenidos");
            if (!estrategiasConforme && string.IsNullOrWhiteSpace(estrategiasComentario)) errores.Add("Estrategias");
            if (!evaluacionConforme && string.IsNullOrWhiteSpace(evaluacionComentario)) errores.Add("Evaluación");
            if (!bibliografiaConforme && string.IsNullOrWhiteSpace(bibliografiaComentario)) errores.Add("Bibliografía");

            if (errores.Any())
            {
                TempData["Error"] = $"Debe agregar un comentario para las secciones 'No conforme': {string.Join(", ", errores)}.";
                return RedirectToAction("DetalleValidacion", new { id });
            }

            _context.ValidacionesAcademicas.Add(new ValidacionAcademica
            {
                SilaboId = id,
                RevisorId = revisor.Id,
                RevisorEmail = email!,
                DatosGeneralesConforme = datosGeneralesConforme,
                DatosGeneralesComentario = datosGeneralesComentario,
                SumillaConforme = sumillaConforme,
                SumillaComentario = sumillaComentario,
                CompetenciasConforme = competenciasConforme,
                CompetenciasComentario = competenciasComentario,
                CapacidadesConforme = capacidadesConforme,
                CapacidadesComentario = capacidadesComentario,
                ContenidosConforme = contenidosConforme,
                ContenidosComentario = contenidosComentario,
                EstrategiasConforme = estrategiasConforme,
                EstrategiasComentario = estrategiasComentario,
                EvaluacionConforme = evaluacionConforme,
                EvaluacionComentario = evaluacionComentario,
                BibliografiaConforme = bibliografiaConforme,
                BibliografiaComentario = bibliografiaComentario,
                Resultado = "Aprobado Final",
                ObservacionGeneral = observacionGeneral,
                Fecha = DateTime.Now,
                Hora = DateTime.Now.ToString("HH:mm")
            });

            silabo.Estado = "Aprobado Final";
            silabo.FechaValidacion = DateTime.Now;
            silabo.ValidadoPor = email!;
            silabo.FechaModificacion = DateTime.Now;

            _context.BitacoraAcciones.Add(new BitacoraAccion
            {
                SilaboId = id,
                CursoId = silabo.CursoId,
                Usuario = email!,
                Rol = "RevisorAcademico",
                Accion = "Sílabo aprobado definitivamente por Departamento Académico",
                Fecha = DateTime.Now,
                Hora = DateTime.Now.ToString("HH:mm")
            });

            await _context.SaveChangesAsync();
            TempData["Exito"] = "Sílabo aprobado definitivamente. Proceso finalizado.";
            return RedirectToAction("ValidacionIndex");
        }

        [HttpPost]
        [Authorize(Roles = "RevisorAcademico")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObservarSilabo(int id,
            bool datosGeneralesConforme, string? datosGeneralesComentario,
            bool sumillaConforme, string? sumillaComentario,
            bool competenciasConforme, string? competenciasComentario,
            bool capacidadesConforme, string? capacidadesComentario,
            bool contenidosConforme, string? contenidosComentario,
            bool estrategiasConforme, string? estrategiasComentario,
            bool evaluacionConforme, string? evaluacionComentario,
            bool bibliografiaConforme, string? bibliografiaComentario,
            string? observacionGeneral)
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var revisor = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (revisor == null) return Unauthorized();

            var silabo = await _context.Silabos.FindAsync(id);
            if (silabo == null) return NotFound();

            // At least one section must be No Conforme
            var noConformes = new List<string>();
            if (!datosGeneralesConforme) noConformes.Add("Datos Generales");
            if (!sumillaConforme) noConformes.Add("Sumilla");
            if (!competenciasConforme) noConformes.Add("Competencias");
            if (!capacidadesConforme) noConformes.Add("Capacidades");
            if (!contenidosConforme) noConformes.Add("Contenidos");
            if (!estrategiasConforme) noConformes.Add("Estrategias");
            if (!evaluacionConforme) noConformes.Add("Evaluación");
            if (!bibliografiaConforme) noConformes.Add("Bibliografía");

            // Validate comments for No Conforme items
            var errores = new List<string>();
            if (!datosGeneralesConforme && string.IsNullOrWhiteSpace(datosGeneralesComentario)) errores.Add("Datos Generales");
            if (!sumillaConforme && string.IsNullOrWhiteSpace(sumillaComentario)) errores.Add("Sumilla");
            if (!competenciasConforme && string.IsNullOrWhiteSpace(competenciasComentario)) errores.Add("Competencias");
            if (!capacidadesConforme && string.IsNullOrWhiteSpace(capacidadesComentario)) errores.Add("Capacidades");
            if (!contenidosConforme && string.IsNullOrWhiteSpace(contenidosComentario)) errores.Add("Contenidos");
            if (!estrategiasConforme && string.IsNullOrWhiteSpace(estrategiasComentario)) errores.Add("Estrategias");
            if (!evaluacionConforme && string.IsNullOrWhiteSpace(evaluacionComentario)) errores.Add("Evaluación");
            if (!bibliografiaConforme && string.IsNullOrWhiteSpace(bibliografiaComentario)) errores.Add("Bibliografía");

            if (errores.Any())
            {
                TempData["Error"] = $"Debe agregar un comentario para las secciones 'No conforme': {string.Join(", ", errores)}.";
                return RedirectToAction("DetalleValidacion", new { id });
            }

            _context.ValidacionesAcademicas.Add(new ValidacionAcademica
            {
                SilaboId = id,
                RevisorId = revisor.Id,
                RevisorEmail = email!,
                DatosGeneralesConforme = datosGeneralesConforme,
                DatosGeneralesComentario = datosGeneralesComentario,
                SumillaConforme = sumillaConforme,
                SumillaComentario = sumillaComentario,
                CompetenciasConforme = competenciasConforme,
                CompetenciasComentario = competenciasComentario,
                CapacidadesConforme = capacidadesConforme,
                CapacidadesComentario = capacidadesComentario,
                ContenidosConforme = contenidosConforme,
                ContenidosComentario = contenidosComentario,
                EstrategiasConforme = estrategiasConforme,
                EstrategiasComentario = estrategiasComentario,
                EvaluacionConforme = evaluacionConforme,
                EvaluacionComentario = evaluacionComentario,
                BibliografiaConforme = bibliografiaConforme,
                BibliografiaComentario = bibliografiaComentario,
                Resultado = "Observado",
                ObservacionGeneral = observacionGeneral,
                Fecha = DateTime.Now,
                Hora = DateTime.Now.ToString("HH:mm")
            });

            silabo.Estado = "Observado";
            silabo.FechaModificacion = DateTime.Now;

            _context.BitacoraAcciones.Add(new BitacoraAccion
            {
                SilaboId = id,
                CursoId = silabo.CursoId,
                Usuario = email!,
                Rol = "RevisorAcademico",
                Accion = $"Sílabo observado — secciones: {string.Join(", ", noConformes)}",
                Fecha = DateTime.Now,
                Hora = DateTime.Now.ToString("HH:mm")
            });

            await _context.SaveChangesAsync();
            TempData["Exito"] = "Sílabo observado. El Director fue notificado para reabrir edición si es necesario.";
            return RedirectToAction("ValidacionIndex");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "RevisorAcademico")]
        public async Task<IActionResult> ReabrirEdicionRevisor(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var silabo = await _context.Silabos.FindAsync(id);

            if (silabo == null) return NotFound();
            
            // Allow reopening if it's Aprobado Final
            if (silabo.Estado == "Aprobado Final")
            {
                silabo.Estado = "En edición";
                silabo.FechaModificacion = DateTime.Now;

                _context.BitacoraAcciones.Add(new BitacoraAccion
                {
                    SilaboId = silabo.Id,
                    CursoId = silabo.CursoId,
                    Usuario = email ?? "Sistema",
                    Rol = "RevisorAcademico",
                    Accion = "Reapertura de edición habilitada (Estado reiniciado a 'En edición')",
                    Fecha = DateTime.Now,
                    Hora = DateTime.Now.ToString("HH:mm")
                });

                await _context.SaveChangesAsync();
                TempData["Exito"] = "Edición habilitada correctamente. El sílabo ha regresado al estado 'En edición'.";
            }
            else
            {
                TempData["Error"] = "No se puede reabrir la edición de este sílabo porque no está en estado 'Aprobado Final'.";
            }

            return RedirectToAction("ValidacionIndex");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "RevisorAcademico")]
        public async Task<IActionResult> ReabrirTodosRevisor()
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            
            var silabosAprobadosFinal = await _context.Silabos
                .Where(s => s.Estado == "Aprobado Final")
                .ToListAsync();

            if (!silabosAprobadosFinal.Any())
            {
                TempData["Error"] = "No hay sílabos en estado 'Aprobado Final' para reabrir.";
                return RedirectToAction("ValidacionIndex");
            }

            foreach (var silabo in silabosAprobadosFinal)
            {
                silabo.Estado = "En edición";
                silabo.FechaModificacion = DateTime.Now;

                _context.BitacoraAcciones.Add(new BitacoraAccion
                {
                    SilaboId = silabo.Id,
                    CursoId = silabo.CursoId,
                    Usuario = email ?? "Sistema",
                    Rol = "RevisorAcademico",
                    Accion = "Reapertura de edición masiva habilitada (Estado reiniciado a 'En edición')",
                    Fecha = DateTime.Now,
                    Hora = DateTime.Now.ToString("HH:mm")
                });
            }

            await _context.SaveChangesAsync();
            TempData["Exito"] = $"Se han habilitado para edición {silabosAprobadosFinal.Count} sílabo(s).";
            
            return RedirectToAction("ValidacionIndex");
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private async Task GuardarObservaciones(int silaboId, User director, string? comentarioGeneral,
            string? obsGenerales, string? obsSumilla, string? obsCompetencias,
            string? obsCapacidades, string? obsContenidos, string? obsEstrategias,
            string? obsEvaluacion, string? obsBibliografia)
        {
            var secciones = new Dictionary<string, string?>
            {
                { "Datos Generales", obsGenerales },
                { "Sumilla", obsSumilla },
                { "Competencias", obsCompetencias },
                { "Capacidades", obsCapacidades },
                { "Programación de Contenidos", obsContenidos },
                { "Estrategias Didácticas", obsEstrategias },
                { "Sistema de Evaluación", obsEvaluacion },
                { "Bibliografía", obsBibliografia }
            };

            foreach (var kvp in secciones)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Value))
                {
                    _context.ObservacionesDirector.Add(new ObservacionDirector
                    {
                        SilaboId = silaboId,
                        DirectorId = director.Id,
                        SeccionObservada = kvp.Key,
                        Comentario = kvp.Value,
                        ComentarioGeneral = comentarioGeneral ?? string.Empty,
                        Fecha = DateTime.Now,
                        Hora = DateTime.Now.ToString("HH:mm"),
                        DirectorEmail = director.Email
                    });
                }
            }

            await Task.CompletedTask;
        }
    }
}
