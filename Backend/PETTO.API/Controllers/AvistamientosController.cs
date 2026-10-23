
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PETTO.API.Data;
using PETTO.API.DTOs;
using PETTO.API.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace PETTO.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AvistamientosController : ControllerBase
    {
        private readonly PettoDbContext _context;

        public AvistamientosController(PettoDbContext context)
        {
            _context = context;
        }

        // GET: api/Avistamientos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Avistamiento>>>
            ObtenerAvistamientos()
        {
            var idUsuarioTexto =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idUsuarioTexto, out int idUsuario))
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario autenticado."
                });
            }

            var avistamientos = await _context.Avistamientos
                .Where(a => a.IdUsuario == idUsuario)
                .ToListAsync();

            return Ok(avistamientos);
        }

        // GET: api/Avistamientos/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Avistamiento>>
            ObtenerAvistamiento(int id)
        {
            var idUsuarioTexto =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idUsuarioTexto, out int idUsuario))
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario autenticado."
                });
            }

            var avistamiento = await _context.Avistamientos
                .FirstOrDefaultAsync(a =>
                    a.IdAvistamiento == id &&
                    a.IdUsuario == idUsuario);

            if (avistamiento == null)
            {
                return NotFound(new
                {
                    mensaje = "Avistamiento no encontrado."
                });
            }

            return Ok(avistamiento);
        }

        // POST: api/Avistamientos
        [HttpPost]
        public async Task<ActionResult<Avistamiento>> CrearAvistamiento(
            [FromBody] CrearAvistamientoDto datos)
        {
            var idUsuarioTexto =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idUsuarioTexto, out int idUsuario))
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario autenticado."
                });
            }

            if (datos.Fecha == null || datos.Hora == null)
            {
                return BadRequest(new
                {
                    mensaje = "La fecha y la hora son obligatorias."
                });
            }

            if (string.IsNullOrWhiteSpace(datos.Ubicacion))
            {
                return BadRequest(new
                {
                    mensaje = "Debes indicar la ubicación del avistamiento."
                });
            }

            if (datos.Fecha.Value.Date > DateTime.Today)
            {
                return BadRequest(new
                {
                    mensaje = "La fecha del avistamiento no puede ser futura."
                });
            }

            var reporte = await _context.ReportesExtravio
                .FirstOrDefaultAsync(r =>
                    r.IdReporte == datos.IdReporte &&
                    r.Estado == "ACTIVO");

            if (reporte == null)
            {
                return BadRequest(new
                {
                    mensaje = "El reporte no existe o ya no está activo."
                });
            }

            var avistamiento = new Avistamiento
            {
                IdReporte = datos.IdReporte,
                IdUsuario = idUsuario,
                Fecha = datos.Fecha.Value,
                Hora = datos.Hora.Value,
                Ubicacion = datos.Ubicacion.Trim(),
                Latitud = datos.Latitud,
                Longitud = datos.Longitud,
                Descripcion = datos.Descripcion?.Trim()
            };

            _context.Avistamientos.Add(avistamiento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObtenerAvistamiento),
                new { id = avistamiento.IdAvistamiento },
                avistamiento
            );
        }
        
        // POST: api/Avistamientos/1/foto
        [HttpPost("{id}/foto")]
        [RequestSizeLimit(5_000_000)]
        public async Task<IActionResult> SubirFoto(
            int id,
            IFormFile foto,
            [FromServices] IWebHostEnvironment entorno)
        {
            var idUsuarioTexto =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idUsuarioTexto, out int idUsuario))
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario autenticado."
                });
            }

            if (foto == null || foto.Length == 0)
            {
                return BadRequest(new
                {
                    mensaje = "Debes seleccionar una fotografía."
                });
            }

            if (foto.Length > 5_000_000)
            {
                return BadRequest(new
                {
                    mensaje = "La fotografía no puede superar los 5 MB."
                });
            }

            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                return BadRequest(new
                {
                    mensaje = "Solo se permiten imágenes JPG, PNG o WEBP."
                });
            }

            var avistamiento = await _context.Avistamientos
                .FirstOrDefaultAsync(a =>
                    a.IdAvistamiento == id &&
                    a.IdUsuario == idUsuario);

            if (avistamiento == null)
            {
                return NotFound(new
                {
                    mensaje = "No se encontró el avistamiento para este usuario."
                });
            }

            var carpeta = Path.Combine(
                entorno.WebRootPath ?? Path.Combine(entorno.ContentRootPath, "wwwroot"),
                "fotos",
                "avistamientos"
            );

            Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
            var rutaFisica = Path.Combine(carpeta, nombreArchivo);

            await using (var stream = new FileStream(rutaFisica, FileMode.CreateNew))
            {
                await foto.CopyToAsync(stream);
            }

            var fotoAvistamiento = new FotoAvistamiento
            {
                IdAvistamiento = id,
                RutaFoto = $"/fotos/avistamientos/{nombreArchivo}"
            };

            _context.FotosAvistamientos.Add(fotoAvistamiento);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (System.IO.File.Exists(rutaFisica))
                {
                    System.IO.File.Delete(rutaFisica);
                }

                throw;
            }

            return Ok(new
            {
                mensaje = "Fotografía guardada correctamente.",
                rutaFoto = fotoAvistamiento.RutaFoto
            });
        }

    }
}
