
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETTO.API.Data;
using PETTO.API.DTOs;
using PETTO.API.Models;

namespace PETTO.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var avistamientos =
                await _context.Avistamientos.ToListAsync();

            return Ok(avistamientos);
        }

        // GET: api/Avistamientos/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Avistamiento>>
            ObtenerAvistamiento(int id)
        {
            var avistamiento =
                await _context.Avistamientos.FindAsync(id);

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
            var reporteExiste = await _context.ReportesExtravio
                .AnyAsync(r => r.IdReporte == datos.IdReporte);

            if (!reporteExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El reporte de extravío indicado no existe."
                });
            }

            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.IdUsuario == datos.IdUsuario);

            if (!usuarioExiste)
            {
                return BadRequest(new
                {
                    mensaje = "El usuario indicado no existe."
                });
            }

            var avistamiento = new Avistamiento
            {
                IdReporte = datos.IdReporte,
                IdUsuario = datos.IdUsuario,
                Fecha = datos.Fecha!.Value,
                Hora = datos.Hora!.Value,
                Ubicacion = datos.Ubicacion?.Trim(),
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
    }
}
