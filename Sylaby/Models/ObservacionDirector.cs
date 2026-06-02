using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sylaby.Models
{
    public class ObservacionDirector
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SilaboId { get; set; }

        [ForeignKey("SilaboId")]
        public Silabo? Silabo { get; set; }

        [Required]
        public int DirectorId { get; set; }

        [ForeignKey("DirectorId")]
        public User? Director { get; set; }

        // Sección observada: Datos Generales, Sumilla, Competencias, etc.
        public string SeccionObservada { get; set; } = string.Empty;

        public string Comentario { get; set; } = string.Empty;

        public string ComentarioGeneral { get; set; } = string.Empty;

        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Hora { get; set; } = DateTime.Now.ToString("HH:mm");
        public string DirectorEmail { get; set; } = string.Empty;
    }
}
