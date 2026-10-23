
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETTO.API.Data;
using PETTO.API.DTOs;
using PETTO.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace PETTO.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class MascotasController : ControllerBase
    {
        private readonly PettoDbContext _context;

        public MascotasController(PettoDbContext context)
        {
            _context = context;
        }

        private int ObtenerIdUsuarioAutenticado()
        {
            var id=User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(id, out int idUsuario))
            {
                throw new UnauthorizedAccessException("No se pudo identificar el usuario autenticado.");
            }

            return idUsuario;
        }

        // GET: api/Mascotas
        [HttpGet]
        public async Task<IActionResult> ObtenerMascotas()
        {
            var idUsuario = ObtenerIdUsuarioAutenticado();

            var mascotas = await _context.Mascotas
                .AsNoTracking()
                .Where(m => m.IdUsuario == idUsuario)
                .Select(m => new
                {
                    m.IdMascota,
                    m.IdUsuario,
                    m.Nombre,
                    m.Especie,
                    m.Raza,
                    m.Sexo,
                    m.FechaNacimiento,
                    m.Color,
                    m.Tamaño,
                    m.Peso,
                    m.Caracteristicas,
                    m.Estado,

                    RutaFoto = _context.FotosMascotas
                        .Where(f => f.IdMascota == m.IdMascota)
                        .OrderByDescending(f => f.EsPrincipal)
                        .ThenBy(f => f.IdFoto)
                        .Select(f => f.RutaFoto)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(mascotas);
        }

        // GET: api/Mascotas/1  
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerMascota(int id)
        {
            var idUsuario = ObtenerIdUsuarioAutenticado();

            var mascota = await _context.Mascotas
                .AsNoTracking()
                .Where(m => m.IdMascota == id && m.IdUsuario == idUsuario)
                .Select(m => new
                {
                    m.IdMascota,
                    m.IdUsuario,
                    m.Nombre,
                    m.Especie,
                    m.Raza,
                    m.Sexo,
                    m.FechaNacimiento,
                    m.Color,
                    m.Tamaño,
                    m.Peso,
                    m.Caracteristicas,
                    m.Estado,

                    RutaFoto = _context.FotosMascotas
                        .Where(f => f.IdMascota == m.IdMascota)
                        .OrderByDescending(f => f.EsPrincipal)
                        .ThenBy(f => f.IdFoto)
                        .Select(f => f.RutaFoto)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (mascota == null)
            {
                return NotFound(new
                {
                    mensaje = "Mascota no encontrada."
                });
            }

            return Ok(mascota);
        }
        // POST: api/Mascotas/5/foto
        [HttpPost("{id}/foto")]
        [RequestSizeLimit(5_000_000)]
        public async Task<IActionResult> SubirFotoMascota(
            int id,
            IFormFile foto,
            [FromServices] IWebHostEnvironment entorno)
        {
            var idUsuario = ObtenerIdUsuarioAutenticado();

            // Comprobar que la mascota pertenece al usuario
            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m =>
                    m.IdMascota == id &&
                    m.IdUsuario == idUsuario);

            if (mascota == null)
            {
                return NotFound(new
                {
                    mensaje = "No se encontró la mascota."
                });
            }

            // Validar la fotografía
            if (foto == null || foto.Length == 0)
            {
                return BadRequest(new
                {
                    mensaje = "Seleccioná una fotografía."
                });
            }

            if (foto.Length > 5_000_000)
            {
                return BadRequest(new
                {
                    mensaje = "La fotografía no puede superar los 5 MB."
                });
            }

            // Comprobar que el archivo sea una imagen admitida
            var extensionesPermitidas = new[]
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                return BadRequest(new
                {
                    mensaje = "Solo se permiten imágenes JPG, PNG o WEBP."
                });
            }

            // Crear la carpeta donde se guardarán las imágenes
            var carpetaWeb = entorno.WebRootPath ??
                Path.Combine(entorno.ContentRootPath, "wwwroot");

            var carpetaFotos = Path.Combine(
                carpetaWeb,
                "fotos",
                "mascotas"
            );

            Directory.CreateDirectory(carpetaFotos);

            // Generar un nombre único
            var nombreArchivo = Guid.NewGuid().ToString("N") + extension;

            var rutaCompleta = Path.Combine(
                carpetaFotos,
                nombreArchivo
            );

            // Guardar la imagen físicamente
            using (var stream = new FileStream(
                rutaCompleta,
                FileMode.CreateNew))
            {
                await foto.CopyToAsync(stream);
            }

            // Crear la ruta pública
            var rutaFoto = "/fotos/mascotas/" + nombreArchivo;

            // Comprobar si ya existe una fotografía principal
            var tienePrincipal = await _context.FotosMascotas
                .AnyAsync(f =>
                    f.IdMascota == id &&
                    f.EsPrincipal);

            // Registrar la fotografía en SQL Server
            var nuevaFoto = new FotoMascota
            {
                IdMascota = id,
                RutaFoto = rutaFoto,
                EsPrincipal = !tienePrincipal
            };

            _context.FotosMascotas.Add(nuevaFoto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                System.IO.File.Delete(rutaCompleta);
                throw;
            }

            return Ok(new
            {
                mensaje = "Fotografía guardada correctamente.",
                idFoto = nuevaFoto.IdFoto,
                rutaFoto = nuevaFoto.RutaFoto
            });
        }

                // POST: api/Mascotas
        [HttpPost]
        public async Task<ActionResult<Mascota>> CrearMascota(
            [FromBody] CrearMascotaDto datos)
        {
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.IdUsuario == ObtenerIdUsuarioAutenticado());

            if (!usuarioExiste)
                return BadRequest(new { mensaje = "El usuario indicado no existe." });

            var mascota = new Mascota
            {
                IdUsuario = ObtenerIdUsuarioAutenticado(),
                Nombre = datos.Nombre.Trim(),
                Especie = datos.Especie.Trim(),
                Raza = datos.Raza?.Trim(),
                Sexo = datos.Sexo?.Trim(),
                FechaNacimiento = datos.FechaNacimiento,
                Color = datos.Color?.Trim(),
                Tamaño = datos.Tamaño?.Trim(),
                Peso = datos.Peso,
                Caracteristicas = datos.Caracteristicas?.Trim(),
                Estado = "REGISTRADA"   // lo decide el servidor, no el cliente
            };

            _context.Mascotas.Add(mascota);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObtenerMascota),
                new { id = mascota.IdMascota },
                mascota
            );
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarMascota(
            int id,
            [FromBody] CrearMascotaDto datos)
        {
            var idUsuario = ObtenerIdUsuarioAutenticado();
            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m => m.IdMascota == id && m.IdUsuario == idUsuario);


            if (mascota == null)
                return NotFound(new { mensaje = "No se encontró la mascota." });

            mascota.Nombre = datos.Nombre.Trim();
            mascota.Especie = datos.Especie.Trim();
            mascota.Raza = datos.Raza?.Trim();
            mascota.Sexo = datos.Sexo?.Trim();
            mascota.FechaNacimiento = datos.FechaNacimiento;
            mascota.Color = datos.Color?.Trim();
            mascota.Tamaño = datos.Tamaño?.Trim();
            mascota.Peso = datos.Peso;
            mascota.Caracteristicas = datos.Caracteristicas?.Trim();

            // El estado no se modifica desde este endpoint.
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Mascota actualizada correctamente.",
                idMascota = mascota.IdMascota
            });
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarMascota(int id)
        {
            var idUsuario = ObtenerIdUsuarioAutenticado();
            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m => m.IdMascota == id && m.IdUsuario == idUsuario);

            if (mascota == null)
                return NotFound(new { mensaje = "No se encontró la mascota." });

            var tieneReportes = await _context.ReportesExtravio
                .AnyAsync(r => r.IdMascota == id);

            if (tieneReportes)
            {
                return Conflict(new
                {
                    mensaje = "No se puede eliminar esta mascota porque tiene reportes de extravío asociados."
                });
            }

            _context.Mascotas.Remove(mascota);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Mascota eliminada correctamente."
            });
        }

    }
}
