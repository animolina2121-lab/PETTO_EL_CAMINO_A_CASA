using System.ComponentModel.DataAnnotations;

namespace PETTO.API.DTOs
{
    public class CrearReporteExtravioDto
    {
        [Range(1, int.MaxValue)]
        public int IdMascota { get; set; }

        [Required]
        public DateTime? Fecha { get; set; }

        [Required]
        public TimeSpan? Hora { get; set; }

        [StringLength(300)]
        public string? Ubicacion { get; set; }

        [Range(-90, 90)]
        public decimal? Latitud { get; set; }

        [Range(-180, 180)]
        public decimal? Longitud { get; set; }

        [StringLength(1000)]
        public string? Descripcion { get; set; }
    }
}