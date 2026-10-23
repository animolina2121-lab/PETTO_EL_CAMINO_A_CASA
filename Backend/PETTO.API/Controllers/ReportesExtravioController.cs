
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETTO.API.Data;
using PETTO.API.DTOs;
using PETTO.API.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PETTO.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesExtravioController : ControllerBase
    {
        private readonly PettoDbContext _context;

        public ReportesExtravioController(PettoDbContext context)
        {
            _context = context;
        }

        // GET: api/ReportesExtravio
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReporteExtravio>>>
            ObtenerReportes()
        {
            var reportes = await _context.ReportesExtravio.ToListAsync();
            return Ok(reportes);
        }

        // GET: api/ReportesExtravio/1
        [HttpGet("{id}")]
        public async Task<ActionResult<ReporteExtravio>>
            ObtenerReporte(int id)
        {
            var reporte = await _context.ReportesExtravio.FindAsync(id);

            if (reporte == null)
            {
                return NotFound(new
                {
                    mensaje = "Reporte de extravío no encontrado."
                });
            }

            return Ok(reporte);
        }

        // POST: api/ReportesExtravio
            [HttpPost]
            [Authorize]
            public async Task<ActionResult<ReporteExtravio>> CrearReporte(
            [FromBody] CrearReporteExtravioDto datos)
        {
            var idUsuarioTexto = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
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
                    mensaje = "La fecha y la hora del extravío son obligatorias."
                });
            }
        
            if (string.IsNullOrWhiteSpace(datos.Ubicacion))
            {
                return BadRequest(new
                {
                    mensaje = "Debes indicar la ubicación del extravío."
                });
            }
        
            if (datos.Fecha.Value.Date > DateTime.Today)
            {
                return BadRequest(new
                {
                    mensaje = "La fecha del extravío no puede ser futura."
                });
            }
        
            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m =>
                    m.IdMascota == datos.IdMascota &&
                    m.IdUsuario == idUsuario);
        
            if (mascota == null)
            {
                return BadRequest(new
                {
                    mensaje = "La mascota no existe o no pertenece al usuario autenticado."
                });
            }
        
            var reporte = new ReporteExtravio
            {
                IdMascota = datos.IdMascota,
                Fecha = datos.Fecha.Value,
                Hora = datos.Hora.Value,
                Ubicacion = datos.Ubicacion.Trim(),
                Latitud = datos.Latitud,
                Longitud = datos.Longitud,
                Descripcion = datos.Descripcion?.Trim(),
                Estado = "ACTIVO"
            };
        
            _context.ReportesExtravio.Add(reporte);
            await _context.SaveChangesAsync();
        
            return CreatedAtAction(
                nameof(ObtenerReporte),
                new { id = reporte.IdReporte },
                reporte
            );
        }
        
    }
}
