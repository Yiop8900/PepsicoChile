using Microsoft.EntityFrameworkCore;
using PepsicoChile.Models;

namespace PepsicoChile.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<IngresoTaller> IngresosTaller { get; set; }
        public DbSet<TareaTaller> TareasTaller { get; set; }
        public DbSet<Pausa> Pausas { get; set; }
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<Repuesto> Repuestos { get; set; }
        public DbSet<DocumentoVehiculo> DocumentosVehiculo { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>(entity =>
              {
                  entity.ToTable("Usuarios");
                  entity.HasKey(e => e.Id);
                  entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                  entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                  entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                  entity.Property(e => e.Telefono).HasMaxLength(20);
                  entity.Property(e => e.Rol).IsRequired().HasMaxLength(50);
                  entity.Property(e => e.Rut).IsRequired().HasMaxLength(20);
                  entity.Property(e => e.Password).IsRequired().HasMaxLength(255);

                  // Índice único para RUT y Email
                  entity.HasIndex(e => e.Rut).IsUnique();
                  entity.HasIndex(e => e.Email).IsUnique();
              });

            // Configuración de Vehiculo
            modelBuilder.Entity<Vehiculo>(entity =>
            {
                  entity.ToTable("Vehiculos");
                  entity.HasKey(e => e.Id);
                  entity.Property(e => e.Patente).IsRequired().HasMaxLength(10);
                  entity.Property(e => e.Marca).IsRequired().HasMaxLength(50);
                  entity.Property(e => e.Modelo).IsRequired().HasMaxLength(50);
                  entity.Property(e => e.TipoVehiculo).IsRequired().HasMaxLength(50);
                  entity.Property(e => e.Estado).IsRequired().HasMaxLength(50);

                  entity.HasIndex(e => e.Patente).IsUnique();
            });

            // Configuración de IngresoTaller
            modelBuilder.Entity<IngresoTaller>(entity =>
                  {
                      entity.ToTable("IngresosTaller");
                      entity.HasKey(e => e.Id);

                      entity.HasOne(e => e.Vehiculo)
                 .WithMany()
                   .HasForeignKey(e => e.VehiculoId)
         .OnDelete(DeleteBehavior.Restrict);

                      entity.HasOne(e => e.Chofer)
            .WithMany()
             .HasForeignKey(e => e.ChoferId)
        .OnDelete(DeleteBehavior.Restrict);

                      entity.HasOne(e => e.Supervisor)
                     .WithMany()
      .HasForeignKey(e => e.SupervisorId)
                  .OnDelete(DeleteBehavior.Restrict);
                  });

            // Configuración de TareaTaller
            modelBuilder.Entity<TareaTaller>(entity =>
               {
                   entity.ToTable("TareasTaller");
                   entity.HasKey(e => e.Id);

                   entity.HasOne(e => e.IngresoTaller)
            .WithMany()
     .HasForeignKey(e => e.IngresoTallerId)
          .OnDelete(DeleteBehavior.Cascade);

                   entity.HasOne(e => e.MecanicoAsignado)
               .WithMany()
            .HasForeignKey(e => e.MecanicoAsignadoId)
          .OnDelete(DeleteBehavior.Restrict);
               });

            // Configuración de Pausa
            modelBuilder.Entity<Pausa>(entity =>
                    {
                        entity.ToTable("Pausas");
                        entity.HasKey(e => e.Id);

                        entity.HasOne(e => e.IngresoTaller)
                  .WithMany()
                     .HasForeignKey(e => e.IngresoTallerId)
              .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(e => e.UsuarioRegistro)
                 .WithMany()
               .HasForeignKey(e => e.UsuarioRegistroId)
             .OnDelete(DeleteBehavior.Restrict);
                    });

            // Configuración de Documento
            modelBuilder.Entity<Documento>(entity =>
               {
                   entity.ToTable("Documentos");
                   entity.HasKey(e => e.Id);

                   entity.HasOne(e => e.IngresoTaller)
          .WithMany()
              .HasForeignKey(e => e.IngresoTallerId)
                   .OnDelete(DeleteBehavior.Cascade);

                   entity.HasOne(e => e.UsuarioSubida)
                        .WithMany()
                         .HasForeignKey(e => e.UsuarioSubidaId)
                   .OnDelete(DeleteBehavior.Restrict);
               });

            // Configuración de Repuesto
            modelBuilder.Entity<Repuesto>(entity =>
             {
                 entity.ToTable("Repuestos");
                 entity.HasKey(e => e.Id);

                 entity.HasOne(e => e.TareaTaller)
        .WithMany()
               .HasForeignKey(e => e.TareaTallerId)
             .OnDelete(DeleteBehavior.Cascade);
             });

            // Configuración de Notificacion
            modelBuilder.Entity<Notificacion>(entity =>
            {
                entity.ToTable("Notificaciones");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Usuario)
                .WithMany()
  .HasForeignKey(e => e.UsuarioId)
 .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UsuarioId);
     entity.HasIndex(e => new { e.UsuarioId, e.Leida });
            });
        }
    }
}
