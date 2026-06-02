using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sylaby.Models
{
    public class BitacoraAccion
    {
        [Key]
        public int Id { get; set; }

        public int? SilaboId { get; set; }

        [ForeignKey("SilaboId")]
        public Silabo? Silabo { get; set; }

        public int? CursoId { get; set; }

        [Required]
        public string Usuario { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = string.Empty;

        [Required]
        public string Accion { get; set; } = string.Empty;

        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Hora { get; set; } = DateTime.Now.ToString("HH:mm");
    }
}
