using System.ComponentModel.DataAnnotations;

namespace PETTO.API.DTOs
{
    public class CrearMascotaDto
    {
     [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Especie { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Raza { get; set; }

        [StringLength(20)]
        public string? Sexo { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Tamaño { get; set; }

        [Range(0, 999.99)]
        public decimal? Peso { get; set; }

        [StringLength(500)]
        public string? Caracteristicas { get; set; }
    }
}