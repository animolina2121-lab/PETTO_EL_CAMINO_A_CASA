namespace PETTO.API.Models
{
    public class Mascota
    {
        public int IdMascota { get; set; }

        public int IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Especie { get; set; } = string.Empty;

        public string? Raza { get; set; }

        public string? Sexo { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public string? Color { get; set; }

        public string? Tamaño { get; set; }

        public decimal? Peso { get; set; }

        public string? Caracteristicas { get; set; }

        public string? Estado { get; set; }
    }
}