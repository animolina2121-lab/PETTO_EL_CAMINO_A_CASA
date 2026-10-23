
using System.ComponentModel.DataAnnotations;

namespace PETTO.API.DTOs
{
    public class CrearAvistamientoDto
    {
        [Range(1, int.MaxValue)]
        public int IdReporte { get; set; }

        [Required]
        public DateTime? Fecha { get; set; }

        [Required]
        public TimeSpan? Hora { get; set; }

        [Required]
        [StringLength(300)]
        public string? Ubicacion { get; set; }

        [Range(typeof(decimal), "-90", "90")]
        public decimal? Latitud { get; set; }

        [Range(typeof(decimal), "-180", "180")]
        public decimal? Longitud { get; set; }

        [StringLength(1000)]
        public string? Descripcion { get; set; }
    }
}
