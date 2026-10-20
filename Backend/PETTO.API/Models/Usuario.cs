namespace PETTO.API.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Dni { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Contraseña { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public string? Domicilio { get; set; }

        public string? Rol { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}