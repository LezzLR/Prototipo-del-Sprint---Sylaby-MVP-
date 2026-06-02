using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sylaby.Models
{
    /// <summary>
    /// Represents a single checklist item evaluated by the Revisor Académico
    /// during final institutional validation of a syllabus.
    /// </summary>
    public class ValidacionAcademica
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SilaboId { get; set; }

        [ForeignKey("SilaboId")]
        public Silabo? Silabo { get; set; }

        // Who validated
        [Required]
        public int RevisorId { get; set; }

        [ForeignKey("RevisorId")]
        public User? Revisor { get; set; }

        public string RevisorEmail { get; set; } = string.Empty;

        // Checklist items — true = Conforme, false = No conforme
        public bool DatosGeneralesConforme { get; set; }
        public string? DatosGeneralesComentario { get; set; }

        public bool SumillaConforme { get; set; }
        public string? SumillaComentario { get; set; }

        public bool CompetenciasConforme { get; set; }
        public string? CompetenciasComentario { get; set; }

        public bool CapacidadesConforme { get; set; }
        public string? CapacidadesComentario { get; set; }

        public bool ContenidosConforme { get; set; }
        public string? ContenidosComentario { get; set; }

        public bool EstrategiasConforme { get; set; }
        public string? EstrategiasComentario { get; set; }

        public bool EvaluacionConforme { get; set; }
        public string? EvaluacionComentario { get; set; }

        public bool BibliografiaConforme { get; set; }
        public string? BibliografiaComentario { get; set; }

        // Overall decision
        // "Aprobado Final" or "Observado"
        public string Resultado { get; set; } = string.Empty;

        // General remarks
        public string? ObservacionGeneral { get; set; }

        // Audit
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Hora { get; set; } = string.Empty;
    }
}
