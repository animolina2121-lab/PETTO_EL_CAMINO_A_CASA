
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETTO.API.Data;
using PETTO.API.DTOs;
using PETTO.API.Models;

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
        public async Task<ActionResult<IEnumerable<Mascota>>> ObtenerMascotas()
        {
            var idUsuario = ObtenerIdUsuarioAutenticado();
            var mascotas = await _context.Mascotas
            .Where(m => m.IdUsuario == idUsuario)
            .ToListAsync();
            return Ok(mascotas);
        }

        // GET: api/Mascotas/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Mascota>> ObtenerMascota(int id)
        {
            var idUsuario = ObtenerIdUsuarioAutenticado();
            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m => m.IdMascota == id && m.IdUsuario == idUsuario);

            if (mascota == null)
                return NotFound(new { mensaje = "Mascota no encontrada." });

            return Ok(mascota);
        }

                // POST: api/Mascotas
        [HttpPost]
        public async Task<ActionResult<Mascota>> CrearMascota(
            [FromBody] CrearMascotaDto datos)
        {
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.IdUsuario == datos.IdUsuario);

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
