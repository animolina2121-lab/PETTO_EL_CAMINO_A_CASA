using Microsoft.EntityFrameworkCore;
using PETTO.API.Models;

namespace PETTO.API.Data
{
    public class PettoDbContext : DbContext
    {
        public PettoDbContext(DbContextOptions<PettoDbContext> options)
            : base(options)
        {
        }

        // Tablas de la base de datos
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<FotoMascota> FotosMascotas { get; set; }
        public DbSet<ReporteExtravio> ReportesExtravio { get; set; }
        public DbSet<Avistamiento> Avistamientos { get; set; }
        public DbSet<FotoAvistamiento> FotosAvistamientos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Nombres reales de las tablas y columnas SQL Server

            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<Usuario>().Property(x => x.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<Usuario>().Property(x => x.Nombre).HasColumnName("nombre");
            modelBuilder.Entity<Usuario>().Property(x => x.Apellido).HasColumnName("apellido");
            modelBuilder.Entity<Usuario>().Property(x => x.Dni).HasColumnName("dni");
            modelBuilder.Entity<Usuario>().Property(x => x.Email).HasColumnName("email");
            modelBuilder.Entity<Usuario>().Property(x => x.Contraseña).HasColumnName("contraseña");
            modelBuilder.Entity<Usuario>().Property(x => x.Telefono).HasColumnName("telefono");
            modelBuilder.Entity<Usuario>().Property(x => x.Domicilio).HasColumnName("domicilio");
            modelBuilder.Entity<Usuario>().Property(x => x.Rol).HasColumnName("rol");
            modelBuilder.Entity<Usuario>().Property(x => x.FechaRegistro).HasColumnName("fecha_registro");

            modelBuilder.Entity<Mascota>().ToTable("mascotas");
            modelBuilder.Entity<Mascota>().Property(x => x.IdMascota).HasColumnName("id_mascota");
            modelBuilder.Entity<Mascota>().Property(x => x.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<Mascota>().Property(x => x.FechaNacimiento).HasColumnName("fecha_nacimiento");
            modelBuilder.Entity<Mascota>().Property(x => x.Tamaño).HasColumnName("tamaño");
            modelBuilder.Entity<Mascota>().Property(x => x.Caracteristicas).HasColumnName("caracteristicas");

            modelBuilder.Entity<FotoMascota>().ToTable("fotos_mascotas");
            modelBuilder.Entity<FotoMascota>().Property(x => x.IdFoto).HasColumnName("id_foto");
            modelBuilder.Entity<FotoMascota>().Property(x => x.IdMascota).HasColumnName("id_mascota");
            modelBuilder.Entity<FotoMascota>().Property(x => x.RutaFoto).HasColumnName("ruta_foto");
            modelBuilder.Entity<FotoMascota>().Property(x => x.EsPrincipal).HasColumnName("es_principal");

            modelBuilder.Entity<ReporteExtravio>().ToTable("reportes_extravio");
            modelBuilder.Entity<ReporteExtravio>().Property(x => x.IdReporte).HasColumnName("id_reporte");
            modelBuilder.Entity<ReporteExtravio>().Property(x => x.IdMascota).HasColumnName("id_mascota");

            modelBuilder.Entity<Avistamiento>().ToTable("avistamientos");
            modelBuilder.Entity<Avistamiento>().Property(x => x.IdAvistamiento).HasColumnName("id_avistamiento");
            modelBuilder.Entity<Avistamiento>().Property(x => x.IdReporte).HasColumnName("id_reporte");
            modelBuilder.Entity<Avistamiento>().Property(x => x.IdUsuario).HasColumnName("id_usuario");

            modelBuilder.Entity<FotoAvistamiento>().ToTable("fotos_avistamientos");
            modelBuilder.Entity<FotoAvistamiento>().Property(x => x.IdFoto).HasColumnName("id_foto");
            modelBuilder.Entity<FotoAvistamiento>().Property(x => x.IdAvistamiento).HasColumnName("id_avistamiento");
            modelBuilder.Entity<FotoAvistamiento>().Property(x => x.RutaFoto).HasColumnName("ruta_foto");


            // ==============================
            // CLAVES PRIMARIAS
            // ==============================

            modelBuilder.Entity<Usuario>()
                .HasKey(u => u.IdUsuario);

            modelBuilder.Entity<Mascota>()
                .HasKey(m => m.IdMascota);

            modelBuilder.Entity<FotoMascota>()
                .HasKey(f => f.IdFoto);

            modelBuilder.Entity<ReporteExtravio>()
                .HasKey(r => r.IdReporte);

            modelBuilder.Entity<Avistamiento>()
                .HasKey(a => a.IdAvistamiento);

            modelBuilder.Entity<FotoAvistamiento>()
                .HasKey(f => f.IdFoto);

            // ==============================
            // PRECISIÓN DE CAMPOS DECIMAL
            // ==============================

            modelBuilder.Entity<Mascota>()
                .Property(m => m.Peso)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ReporteExtravio>()
                .Property(r => r.Latitud)
                .HasPrecision(9, 6);

            modelBuilder.Entity<ReporteExtravio>()
                .Property(r => r.Longitud)
                .HasPrecision(9, 6);

            modelBuilder.Entity<Avistamiento>()
                .Property(a => a.Latitud)
                .HasPrecision(9, 6);

            modelBuilder.Entity<Avistamiento>()
                .Property(a => a.Longitud)
                .HasPrecision(9, 6);
                
            // ==============================
            // RELACIONES
            // ==============================

            // Usuario → Mascotas
            modelBuilder.Entity<Mascota>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(m => m.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // Mascota → Fotos
            modelBuilder.Entity<FotoMascota>()
                .HasOne<Mascota>()
                .WithMany()
                .HasForeignKey(f => f.IdMascota)
                .OnDelete(DeleteBehavior.Cascade);

            // Mascota → Reportes de extravío
            modelBuilder.Entity<ReporteExtravio>()
                .HasOne<Mascota>()
                .WithMany()
                .HasForeignKey(r => r.IdMascota)
                .OnDelete(DeleteBehavior.NoAction);

            // Reporte → Avistamientos
            modelBuilder.Entity<Avistamiento>()
                .HasOne<ReporteExtravio>()
                .WithMany()
                .HasForeignKey(a => a.IdReporte)
                .OnDelete(DeleteBehavior.NoAction);

            // Usuario → Avistamientos
            modelBuilder.Entity<Avistamiento>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(a => a.IdUsuario)
                .OnDelete(DeleteBehavior.NoAction);

            // Avistamiento → Fotos
            modelBuilder.Entity<FotoAvistamiento>()
                .HasOne<Avistamiento>()
                .WithMany()
                .HasForeignKey(f => f.IdAvistamiento)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}