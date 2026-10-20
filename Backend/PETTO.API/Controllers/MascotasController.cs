
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETTO.API.Data;
using PETTO.API.Models;

namespace PETTO.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MascotasController : ControllerBase
    {
        private readonly PettoDbContext _context;

        public MascotasController(PettoDbContext context)
        {
            _context = context;
        }

        // GET: api/Mascotas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mascota>>> ObtenerMascotas()
        {
            var mascotas = await _context.Mascotas.ToListAsync();
            return Ok(mascotas);
        }

        // GET: api/Mascotas/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Mascota>> ObtenerMascota(int id)
        {
            var mascota = await _context.Mascotas.FindAsync(id);

            if (mascota == null)
                return NotFound(new { mensaje = "Mascota no encontrada." });

            return Ok(mascota);
        }

        // POST: api/Mascotas
        [HttpPost]
        public async Task<ActionResult<Mascota>> CrearMascota(Mascota mascota)
        {
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.IdUsuario == mascota.IdUsuario);

            if (!usuarioExiste)
                return BadRequest(new { mensaje = "El usuario indicado no existe." });

            _context.Mascotas.Add(mascota);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObtenerMascota),
                new { id = mascota.IdMascota },
                mascota
            );
        }
    }
}
