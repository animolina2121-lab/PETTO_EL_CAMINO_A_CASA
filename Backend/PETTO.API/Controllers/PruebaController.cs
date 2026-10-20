using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETTO.API.Data;

namespace PETTO.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PruebaController : ControllerBase
    {
        private readonly PettoDbContext _context;

        public PruebaController(PettoDbContext context)
        {
            _context = context;
        }

       [HttpGet]
public async Task<IActionResult> ProbarConexion()
{
    try
    {
        await _context.Database.OpenConnectionAsync();

        return Ok(new
        {
            mensaje = "Conexión exitosa con SQL Server y la base de datos PETTO."
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            mensaje = "Error al conectar con SQL Server.",
            error = ex.Message,
            detalle = ex.InnerException?.Message
        });
    }
    finally
    {
        await _context.Database.CloseConnectionAsync();
    }
}
    }
}