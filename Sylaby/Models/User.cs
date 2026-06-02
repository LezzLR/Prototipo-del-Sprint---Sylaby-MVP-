using System.ComponentModel.DataAnnotations;

namespace Sylaby.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio.")]
        public string Role { get; set; } = string.Empty; // "Director" o "Docente"

        // Bidirectional relationships for EF Core
        public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
        public ICollection<EncuestaCierreCiclo> Encuestas { get; set; } = new List<EncuestaCierreCiclo>();
    }
}
