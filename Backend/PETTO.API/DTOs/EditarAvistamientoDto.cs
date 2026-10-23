namespace PETTO.API.DTOs
{
    public class EditarAvistamientoDto
    {
        public DateTime? Fecha { get; set; }
        public TimeSpan? Hora { get; set; }
        public string? Ubicacion { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public string? Descripcion { get; set; }
    }
}