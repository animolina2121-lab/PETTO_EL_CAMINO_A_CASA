
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETTO.API.Data;
using PETTO.API.Models;
using System.Security.Claims;

namespace PETTO.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FotosMascotasController : ControllerBase
    {
        private readonly PettoDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public FotosMascotasController(
            PettoDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // POST: api/FotosMascotas/5
        [HttpPost("{idMascota:int}")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> SubirFoto(
            int idMascota,
            [FromForm] IFormFile foto)
        {
            var idTexto = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(idTexto, out int idUsuario))
            {
                return Unauthorized(new
                {
                    mensaje = "Sesión no válida."
                });
            }

            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m =>
                    m.IdMascota == idMascota &&
                    m.IdUsuario == idUsuario
                );

            if (mascota == null)
            {
                return NotFound(new
                {
                    mensaje = "Mascota no encontrada o sin autorización."
                });
            }

            if (foto == null || foto.Length == 0)
            {
                return BadRequest(new
                {
                    mensaje = "Debes seleccionar una fotografía."
                });
            }

            if (foto.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    mensaje = "La fotografía no puede superar los 5 MB."
                });
            }

            var extension = Path.GetExtension(
                foto.FileName
            ).ToLowerInvariant();

            var extensionesPermitidas = new[]
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            if (!extensionesPermitidas.Contains(extension))
            {
                return BadRequest(new
                {
                    mensaje = "Solo se permiten imágenes JPG, PNG o WEBP."
                });
            }

            // Verificar la firma real del archivo.
            byte[] cabecera = new byte[12];

            await using (var lectura = foto.OpenReadStream())
            {
                int bytesLeidos = await lectura.ReadAsync(
                    cabecera.AsMemory(0, cabecera.Length)
                );

                if (bytesLeidos < 12)
                {
                    return BadRequest(new
                    {
                        mensaje = "El archivo no es una imagen válida."
                    });
                }
            }

            bool esJpeg =
                cabecera[0] == 0xFF &&
                cabecera[1] == 0xD8 &&
                cabecera[2] == 0xFF;

            bool esPng =
                cabecera[0] == 0x89 &&
                cabecera[1] == 0x50 &&
                cabecera[2] == 0x4E &&
                cabecera[3] == 0x47 &&
                cabecera[4] == 0x0D &&
                cabecera[5] == 0x0A &&
                cabecera[6] == 0x1A &&
                cabecera[7] == 0x0A;

            bool esWebp =
                System.Text.Encoding.ASCII.GetString(
                    cabecera, 0, 4
                ) == "RIFF" &&
                System.Text.Encoding.ASCII.GetString(
                    cabecera, 8, 4
                ) == "WEBP";

            bool formatoValido = extension switch
            {
                ".jpg" or ".jpeg" => esJpeg,
                ".png" => esPng,
                ".webp" => esWebp,
                _ => false
            };

            if (!formatoValido)
            {
                return BadRequest(new
                {
                    mensaje = "El contenido no coincide con el formato de imagen."
                });
            }

            var carpetaWeb = _environment.WebRootPath
                ?? Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot"
                );

            var carpetaFotos = Path.Combine(
                carpetaWeb,
                "uploads",
                "mascotas"
            );

            Directory.CreateDirectory(carpetaFotos);

            var nombreArchivo =
                Guid.NewGuid().ToString("N") + extension;

            var rutaFisica = Path.Combine(
                carpetaFotos,
                nombreArchivo
            );

            try
            {
                await using (var archivo =
                    new FileStream(
                        rutaFisica,
                        FileMode.CreateNew))
                {
                    await foto.CopyToAsync(archivo);
                }

                var existeFoto = await _context.FotosMascotas
                    .AnyAsync(f => f.IdMascota == idMascota);

                var fotoRegistrada = new FotoMascota
                {
                    IdMascota = idMascota,
                    RutaFoto = "/uploads/mascotas/" + nombreArchivo,
                    EsPrincipal = !existeFoto
                };

                _context.FotosMascotas.Add(fotoRegistrada);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Fotografía guardada correctamente.",
                    fotoRegistrada.IdFoto,
                    fotoRegistrada.IdMascota,
                    fotoRegistrada.RutaFoto,
                    fotoRegistrada.EsPrincipal
                });
            }
            catch
            {
                if (System.IO.File.Exists(rutaFisica))
                {
                    System.IO.File.Delete(rutaFisica);
                }

                throw;
            }
        }
    }
}
