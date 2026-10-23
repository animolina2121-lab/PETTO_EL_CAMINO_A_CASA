
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PETTO.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Conexión con SQL Server.
builder.Services.AddDbContext<PettoDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PettoConnection")
    )
);

// Configuración de autenticación JWT.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "No se encontró la clave Jwt:Key en User Secrets."
    );

var issuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "No se encontró Jwt:Issuer en la configuración."
    );

var audience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "No se encontró Jwt:Audience en la configuración."
    );

var keyBytes = Convert.FromBase64String(jwtKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Servicios de la API.
builder.Services.AddControllers();

// CORS para el frontend de PETTO.
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPETTO", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5500",
                "http://127.0.0.1:5500"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors("FrontendPETTO");

// La autenticación debe ejecutarse antes de la autorización.
app.UseAuthentication();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
