namespace PETTO.API.DTOs
{
    public class RegistroUsuarioDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Contraseña { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Domicilio { get; set; }
    }
}