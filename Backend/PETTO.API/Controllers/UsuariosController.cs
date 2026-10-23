
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PETTO.API.Data;
using PETTO.API.DTOs;
using PETTO.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace PETTO.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly PettoDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher = new();

        private readonly IConfiguration _configuration;

        public UsuariosController(
            PettoDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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


        // GET: api/Usuarios/perfil
        // Devuelve los datos del usuario autenticado.
        [Authorize]
        [HttpGet("perfil")]
        public async Task<IActionResult> ObtenerPerfil()
        {
            var idTexto = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(idTexto, out int idUsuario))
            {
                return Unauthorized(new
                {
                    mensaje = "La sesión no es válida."
                });
            }

            var usuario = await _context.Usuarios
                .Where(u => u.IdUsuario == idUsuario)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nombre,
                    u.Apellido,
                    u.Dni,
                    u.Email,
                    u.Telefono,
                    u.Domicilio,
                    u.Rol,
                    u.FechaRegistro
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensaje = "Usuario no encontrado."
                });
            }

            return Ok(usuario);
        }


        // PUT: api/Usuarios/perfil
        // Actualiza los datos del usuario autenticado.
        [Authorize]
        [HttpPut("perfil")]
        public async Task<IActionResult> ActualizarPerfil(
            [FromBody] ActualizarPerfilDto datos)
        {
            var idTexto = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(idTexto, out int idUsuario))
            {
                return Unauthorized(new
                {
                    mensaje = "La sesión no es válida."
                });
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(
                    u => u.IdUsuario == idUsuario
                );

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensaje = "Usuario no encontrado."
                });
            }

            var email = datos.Email.Trim();

            bool emailOcupado = await _context.Usuarios
                .AnyAsync(u =>
                    u.Email == email &&
                    u.IdUsuario != idUsuario
                );

            if (emailOcupado)
            {
                return Conflict(new
                {
                    mensaje = "Ese correo electrónico ya está registrado."
                });
            }

            usuario.Nombre = datos.Nombre.Trim();
            usuario.Apellido = datos.Apellido.Trim();
            usuario.Dni = datos.Dni.Trim();
            usuario.Email = email;
            usuario.Telefono = datos.Telefono?.Trim();
            usuario.Domicilio = datos.Domicilio?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Perfil actualizado correctamente.",
                usuario = new
                {
                    usuario.IdUsuario,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Dni,
                    usuario.Email,
                    usuario.Telefono,
                    usuario.Domicilio
                }
            });
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

            
            // 3. Obtener la configuración JWT.
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "No se encontró Jwt:Key en User Secrets.");

            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "No se encontró Jwt:Issuer en la configuración.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "No se encontró Jwt:Audience en la configuración.");

            var keyBytes = Convert.FromBase64String(jwtKey);

            // 4. Crear los datos de identidad del usuario.
            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()),

                new Claim(ClaimTypes.Name, usuario.Nombre),

                new Claim(ClaimTypes.Email, usuario.Email),

                new Claim(ClaimTypes.Role, usuario.Rol ?? "usuario")
            };

            // 5. Firmar el token y establecer su vencimiento.
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var credentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var tokenGenerado =
                new JwtSecurityTokenHandler().WriteToken(token);

            // 6. Devolver el token junto con los datos del usuario.
            return Ok(new
            {
                mensaje = "Inicio de sesión correcto.",
                usuario = new
                {
                    idUsuario = usuario.IdUsuario,
                    nombre = usuario.Nombre,
                    apellido = usuario.Apellido,
                    email = usuario.Email,
                    rol = usuario.Rol,
                    token = tokenGenerado
                }
            });

        }
    }
}
