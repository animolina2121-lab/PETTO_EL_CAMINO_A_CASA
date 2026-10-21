
using System.ComponentModel.DataAnnotations;

namespace PETTO.API.DTOs
{
    public class RegistroUsuarioDto
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        public string Dni { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Contraseña { get; set; } = string.Empty;

        public string? Telefono { get; set; }
        public string? Domicilio { get; set; }
    }
}
