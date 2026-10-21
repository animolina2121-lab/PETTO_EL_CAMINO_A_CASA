using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PETTO.API.Data;
using PETTO.API.DTOs;
using PETTO.API.Models;

namespace PETTO.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly PettoDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher = new();

        public UsuariosController(PettoDbContext context)
        {
            _context = context;
        }


        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.IdUsuario == id)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nombre,
                    u.Apellido,
                    u.Email,
                    u.Rol,
                    u.FechaRegistro
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            return Ok(usuario);
        }

        // POST: api/Usuarios
        [HttpPost]
        public async Task<IActionResult> CrearUsuario(
            [FromBody] RegistroUsuarioDto datos)
        {
            if (await _context.Usuarios.AnyAsync(
                u => u.Email == datos.Email))
            {
                return Conflict(new
                {
                    mensaje = "Ese correo ya está registrado."
                });
            }

            var usuario = new Usuario
            {
                Nombre = datos.Nombre.Trim(),
                Apellido = datos.Apellido.Trim(),
                Dni = datos.Dni.Trim(),
                Email = datos.Email.Trim(),
                Telefono = datos.Telefono,
                Domicilio = datos.Domicilio,
                Rol = "usuario",
                FechaRegistro = DateTime.Now
            };

            // Guardamos un hash, nunca la contraseña original.
            usuario.Contraseña = _passwordHasher.HashPassword(
                usuario,
                datos.Contraseña
            );

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObtenerUsuario),
                new { id = usuario.IdUsuario },
                new
                {
                    usuario.IdUsuario,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Email,
                    usuario.FechaRegistro,
                    mensaje = "Cuenta creada correctamente."
                }
            );

        }
        
        // POST: api/Usuarios/login
        [HttpPost("login")]
        public async Task<IActionResult> IniciarSesion(
            [FromBody] LoginDto datos)
        {
            // 1. Buscar el usuario por su correo electrónico.
            var email = datos.Email.Trim();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            // 2. Verificar que exista y que la contraseña sea correcta.
            if (usuario == null)
            {
                return Unauthorized(new
                {
                    mensaje = "Correo o contraseña incorrectos."
                });
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.Contraseña,
                datos.Contraseña
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    mensaje = "Correo o contraseña incorrectos."
                });
            }

            // 3. Devolver solo los datos necesarios para continuar.
            return Ok(new
            {
                mensaje = "Inicio de sesión correcto.",
                usuario = new
                {
                    idUsuario = usuario.IdUsuario,
                    nombre = usuario.Nombre,
                    apellido = usuario.Apellido,
                    email = usuario.Email,
                    rol = usuario.Rol
                }
            });
        }
    }
}
