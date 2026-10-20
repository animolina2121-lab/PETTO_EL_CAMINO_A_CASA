using Microsoft.EntityFrameworkCore;
using PETTO.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Conexión con SQL Server
builder.Services.AddDbContext<PettoDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PettoConnection")
    )
);

// Servicios de la API
builder.Services.AddControllers();

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

app.UseHttpsRedirection();

app.UseCors("FrontendPETTO");

app.UseAuthorization();

app.MapControllers();

app.Run();