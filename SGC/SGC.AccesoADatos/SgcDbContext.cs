using Microsoft.EntityFrameworkCore;
using SGC.Abstracciones.Entidades;
using NombresRol = SGC.Abstracciones.Constantes.Roles;

namespace SGC.AccesoADatos;

public class SgcDbContext : DbContext
{
    public SgcDbContext(DbContextOptions<SgcDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();

    //  agregan aquí sus DbSet 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(r => r.Id);
            e.Property(r => r.Nombre).IsRequired().HasMaxLength(50);
            e.Property(r => r.Descripcion).IsRequired().HasMaxLength(100);
            e.HasIndex(r => r.Nombre).IsUnique();

            e.HasData(
                new Rol { Id = 1, Nombre = NombresRol.Administrador, Descripcion = "Administrador" },
                new Rol { Id = 2, Nombre = NombresRol.ServicioCliente, Descripcion = "Servicio al cliente" },
                new Rol { Id = 3, Nombre = NombresRol.Analista, Descripcion = "Analista" },
                new Rol { Id = 4, Nombre = NombresRol.Gestor, Descripcion = "Gestor" });
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(u => u.Id);
            e.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
            e.Property(u => u.Correo).IsRequired().HasMaxLength(150);
            e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            e.HasIndex(u => u.Correo).IsUnique();

            e.HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
