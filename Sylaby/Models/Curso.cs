using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sylaby.Models
{
    public class Curso
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del curso es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public int DocenteId { get; set; }

        [ForeignKey("DocenteId")]
        public User? Docente { get; set; }

        // Navigation property to access course survey
        public ICollection<EncuestaCierreCiclo> Encuestas { get; set; } = new List<EncuestaCierreCiclo>();

        // Navigation property to access syllabus
        public ICollection<Silabo> Silabos { get; set; } = new List<Silabo>();
    }
}
