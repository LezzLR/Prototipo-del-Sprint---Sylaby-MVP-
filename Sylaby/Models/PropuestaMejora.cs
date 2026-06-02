using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sylaby.Models
{
    public class PropuestaMejora
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SilaboId { get; set; }

        [ForeignKey("SilaboId")]
        public Silabo? Silabo { get; set; }

        [Required]
        public int DocenteId { get; set; }

        [ForeignKey("DocenteId")]
        public User? Docente { get; set; }

        [Required(ErrorMessage = "La sección afectada es obligatoria.")]
        public string SeccionAfectada { get; set; } = string.Empty;

        [Required(ErrorMessage = "La propuesta de actualización es obligatoria.")]
        public string PropuestaActualizacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La justificación es obligatoria.")]
        public string Justificacion { get; set; } = string.Empty;

        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Hora { get; set; } = DateTime.Now.ToString("HH:mm");
        public string Autor { get; set; } = string.Empty;
    }
}
