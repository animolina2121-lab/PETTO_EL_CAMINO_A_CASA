using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETTO.API.Data;
using PETTO.API.DTOs;
using PETTO.API.Models;

namespace PETTO.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PettoDbContext _context;
        private readonly PasswordHasher<Usuario> _hasher = new();

        public AuthController(PettoDbContext context) => _context = context;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto datos)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == datos.Email.Trim());

            var ok = usuario != null &&
                _hasher.VerifyHashedPassword(usuario, usuario.Contraseña, datos.Contraseña)
                    != PasswordVerificationResult.Failed;

            if (!ok)
                return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });

            return Ok(new { usuario!.IdUsuario, usuario.Nombre, usuario.Apellido, usuario.Email, usuario.Rol });
        }
    }
}