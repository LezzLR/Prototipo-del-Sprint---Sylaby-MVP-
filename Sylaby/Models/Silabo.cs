using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sylaby.Models
{
    public class Silabo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }

        [ForeignKey("CursoId")]
        public Curso? Curso { get; set; }

        // ── Datos Generales ──────────────────────────────────────────
        public string EscuelaProfesional { get; set; } = string.Empty;
        public string CicloAcademico { get; set; } = string.Empty;
        public int Creditos { get; set; }
        public int HorasSemanales { get; set; }
        public string DocenteResponsable { get; set; } = string.Empty;

        // ── Secciones de contenido ───────────────────────────────────
        public string Sumilla { get; set; } = string.Empty;
        public string Competencias { get; set; } = string.Empty;
        public string Capacidades { get; set; } = string.Empty;
        public string ProgramacionContenidos { get; set; } = string.Empty;
        public string EstrategiasDiddacticas { get; set; } = string.Empty;
        public string SistemaEvaluacion { get; set; } = string.Empty;
        public string Bibliografia { get; set; } = string.Empty;

        // ── Estado y control de flujo ────────────────────────────────────────────
        // Estados: "En edición", "En revisión", "Por corregir",
        //          "Aprobado por Director", "En validación académica",
        //          "Observado", "Aprobado Final"
        public string Estado { get; set; } = "En edición";
        public bool PermiteNuevasPropuestas { get; set; } = false;

        // ── Auditoría ────────────────────────────────────────────────
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaModificacion { get; set; } = DateTime.Now;
        public string CreadoPor { get; set; } = string.Empty;
        public string ModificadoPor { get; set; } = string.Empty;

        public DateTime? FechaEnvio { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string AprobadoPor { get; set; } = string.Empty;

        public DateTime? FechaRechazo { get; set; }
        public string RechazadoPor { get; set; } = string.Empty;

        // ── Validación Académica (Departamento Académico) ─────────────────────
        public DateTime? FechaValidacion { get; set; }
        public string ValidadoPor { get; set; } = string.Empty;

        // ── Navegación ───────────────────────────────────────────────
        public ICollection<PropuestaMejora> Propuestas { get; set; } = new List<PropuestaMejora>();
        public ICollection<ObservacionDirector> Observaciones { get; set; } = new List<ObservacionDirector>();
        public ICollection<BitacoraAccion> Bitacora { get; set; } = new List<BitacoraAccion>();
        public ICollection<ValidacionAcademica> ValidacionesAcademicas { get; set; } = new List<ValidacionAcademica>();
    }
}
