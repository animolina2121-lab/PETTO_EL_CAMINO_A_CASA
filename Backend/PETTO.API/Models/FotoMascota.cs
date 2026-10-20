namespace PETTO.API.Models
{
    public class FotoMascota
    {
        public int IdFoto { get; set; }

        public int IdMascota { get; set; }

        public string RutaFoto { get; set; } = string.Empty;

        public bool EsPrincipal { get; set; }
    }
}