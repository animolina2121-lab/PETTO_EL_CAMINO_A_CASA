namespace PETTO.API.Models
{
    public class Avistamiento
    {
        public int IdAvistamiento { get; set; }

        public int IdReporte { get; set; }

        public int IdUsuario { get; set; }

        public DateTime Fecha { get; set; }

        public TimeSpan Hora { get; set; }

        public string? Ubicacion { get; set; }

        public decimal? Latitud { get; set; }

        public decimal? Longitud { get; set; }

        public string? Descripcion { get; set; }
    }
}