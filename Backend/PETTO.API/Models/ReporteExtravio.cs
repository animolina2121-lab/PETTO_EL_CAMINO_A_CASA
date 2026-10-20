namespace PETTO.API.Models
{
    public class ReporteExtravio
    {
        public int IdReporte { get; set; }

        public int IdMascota { get; set; }

        public DateTime Fecha { get; set; }

        public TimeSpan Hora { get; set; }

        public string? Ubicacion { get; set; }

        public decimal? Latitud { get; set; }

        public decimal? Longitud { get; set; }

        public string? Descripcion { get; set; }

        public string? Estado { get; set; }
    }
}