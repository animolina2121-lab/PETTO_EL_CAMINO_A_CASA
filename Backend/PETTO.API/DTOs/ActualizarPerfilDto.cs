
using System.ComponentModel.DataAnnotations;

namespace PETTO.API.DTOs
{
    public class ActualizarPerfilDto
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

        public string? Telefono { get; set; }

        public string? Domicilio { get; set; }
    }
}
