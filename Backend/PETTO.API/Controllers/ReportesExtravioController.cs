
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

        // GET: api/ReportesExtravio/mapa
        [HttpGet("mapa")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerReportesMapa()
        {
            var reportes = await (
                from reporte in _context.ReportesExtravio.AsNoTracking()
                join mascota in _context.Mascotas.AsNoTracking()
                    on reporte.IdMascota equals mascota.IdMascota
                where reporte.Estado == "ACTIVO"
                    && reporte.Latitud != null
                    && reporte.Longitud != null
                select new
                {
                    reporte.IdReporte,
                    reporte.IdMascota,
                    mascota.Nombre,
                    mascota.Especie,
                    mascota.Raza,
                    reporte.Fecha,
                    reporte.Hora,
                    reporte.Ubicacion,
                    reporte.Latitud,
                    reporte.Longitud,
                    reporte.Descripcion,
                    reporte.Estado,

                    RutaFoto = _context.FotosMascotas
                        .Where(f => f.IdMascota == mascota.IdMascota)
                        .OrderByDescending(f => f.EsPrincipal)
                        .ThenBy(f => f.IdFoto)
                        .Select(f => f.RutaFoto)
                        .FirstOrDefault()
                }
            ).ToListAsync();

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
        
        // ==========================================
        // PUT: api/ReportesExtravio/1
        // EDITAR REPORTE DE EXTRAVÍO
        // ==========================================
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditarReporte(
            int id,
            [FromBody] EditarReporteExtravioDto datos)
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

            // Buscar el reporte y comprobar que la mascota
            // pertenece al usuario autenticado.
            var reporte = await (
                from r in _context.ReportesExtravio
                join m in _context.Mascotas
                    on r.IdMascota equals m.IdMascota
                where r.IdReporte == id &&
                    m.IdUsuario == idUsuario
                select r
            ).FirstOrDefaultAsync();

            if (reporte == null)
            {
                return NotFound(new
                {
                    mensaje = "El reporte no existe o no pertenece al usuario."
                });
            }

            // Validaciones
            if (datos.Fecha == null || datos.Hora == null)
            {
                return BadRequest(new
                {
                    mensaje = "La fecha y la hora son obligatorias."
                });
            }

            if (datos.Fecha.Value.Date > DateTime.Today)
            {
                return BadRequest(new
                {
                    mensaje = "La fecha del extravío no puede ser futura."
                });
            }

            if (string.IsNullOrWhiteSpace(datos.Ubicacion))
            {
                return BadRequest(new
                {
                    mensaje = "Debes indicar la ubicación del extravío."
                });
            }

            if (datos.Latitud is < -90 or > 90 ||
                datos.Longitud is < -180 or > 180)
            {
                return BadRequest(new
                {
                    mensaje = "Las coordenadas no son válidas."
                });
            }

            var estado = datos.Estado?.Trim().ToUpperInvariant();

            if (estado != "ACTIVO" && estado != "CERRADO")
            {
                return BadRequest(new
                {
                    mensaje = "El estado debe ser ACTIVO o CERRADO."
                });
            }

            // Actualizar los datos.
            reporte.Fecha = datos.Fecha.Value;
            reporte.Hora = datos.Hora.Value;
            reporte.Ubicacion = datos.Ubicacion.Trim();
            reporte.Latitud = datos.Latitud;
            reporte.Longitud = datos.Longitud;
            reporte.Descripcion = datos.Descripcion?.Trim();
            reporte.Estado = estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Reporte actualizado correctamente."
            });
        }

        // ==========================================
        // DELETE: api/ReportesExtravio/1
        // ELIMINAR REPORTE DE EXTRAVÍO
        // ==========================================
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> EliminarReporte(int id)
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

            // Comprobar que el reporte pertenece a una
            // mascota del usuario autenticado.
            var reporte = await (
                from r in _context.ReportesExtravio
                join m in _context.Mascotas
                    on r.IdMascota equals m.IdMascota
                where r.IdReporte == id &&
                    m.IdUsuario == idUsuario
                select r
            ).FirstOrDefaultAsync();

            if (reporte == null)
            {
                return NotFound(new
                {
                    mensaje = "El reporte no existe o no pertenece al usuario."
                });
            }

            // No eliminar reportes con avistamientos asociados.
            var tieneAvistamientos = await _context.Avistamientos
                .AnyAsync(a => a.IdReporte == id);

            if (tieneAvistamientos)
            {
                return Conflict(new
                {
                    mensaje = "Este reporte tiene avistamientos asociados. " +
                            "No puede eliminarse. Podés cambiar su estado a CERRADO."
                });
            }

            _context.ReportesExtravio.Remove(reporte);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Reporte de extravío eliminado correctamente."
            });
        }

    }
}
