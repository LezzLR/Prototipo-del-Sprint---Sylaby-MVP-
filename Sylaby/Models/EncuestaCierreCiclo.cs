using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sylaby.Models
{
    public class EncuestaCierreCiclo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }

        [ForeignKey("CursoId")]
        public Curso? Curso { get; set; }

        [Required]
        public int DocenteId { get; set; }

        [ForeignKey("DocenteId")]
        public User? Docente { get; set; }

        [Required]
        public string DocenteEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "El porcentaje de cumplimiento es obligatorio.")]
        [Range(0, 100, ErrorMessage = "El porcentaje de cumplimiento debe estar entre 0 y 100.")]
        public int PorcentajeCumplimiento { get; set; }

        public string? TemasNoDesarrollados { get; set; }

        public string? ObservacionesFinales { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; }

        [Required]
        public DateTime FechaUltimaModificacion { get; set; }

        public DateTime? FechaEnvioDefinitivo { get; set; }

        [Required]
        public string Estado { get; set; } = "Borrador"; // "Borrador" o "Enviada"
    }
}
