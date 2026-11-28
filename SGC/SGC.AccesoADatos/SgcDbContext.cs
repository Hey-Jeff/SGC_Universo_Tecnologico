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
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<BitacoraEvento> Bitacora => Set<BitacoraEvento>();

    //daniel
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<SolicitudCredito> SolicitudesCredito => Set<SolicitudCredito>();
    public DbSet<DocumentoSolicitudCredito> DocumentosSolicitudCredito => Set<DocumentoSolicitudCredito>();

    // Los demás integrantes agregan aquí sus DbSet (Clientes, Equipos, Articulos...).

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
                new Rol { Id = 1, Nombre = NombresRol.Administrador, Descripcion = "Administrador / Propietario" },
                new Rol { Id = 2, Nombre = NombresRol.ServicioCliente, Descripcion = "Servicio al cliente" },
                new Rol { Id = 3, Nombre = NombresRol.Analista, Descripcion = "Analista" },
                new Rol { Id = 4, Nombre = NombresRol.Gestor, Descripcion = "Gestor / Operativo" });
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(u => u.Id);
            e.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
            e.Property(u => u.Correo).IsRequired().HasMaxLength(150);
            e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            e.HasIndex(u => u.Correo).IsUnique();
        });

        modelBuilder.Entity<UsuarioRol>(e =>
        {
            e.ToTable("UsuarioRoles");
            e.HasKey(ur => new { ur.UsuarioId, ur.RolId });

            e.HasOne(ur => ur.Usuario)
                .WithMany(u => u.UsuarioRoles)
                .HasForeignKey(ur => ur.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ur => ur.Rol)
                .WithMany(r => r.UsuarioRoles)
                .HasForeignKey(ur => ur.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BitacoraEvento>(e =>
        {
            e.ToTable("BitacoraEventos");
            e.HasKey(b => b.Id);
            e.Property(b => b.Entidad).IsRequired().HasMaxLength(60);
            e.Property(b => b.Registro).IsRequired().HasMaxLength(100);
            e.Property(b => b.Accion).IsRequired().HasMaxLength(60);
            e.Property(b => b.Detalle).HasMaxLength(500);
            e.HasIndex(b => b.Fecha);

            e.HasOne(b => b.Usuario)
                .WithMany()
                .HasForeignKey(b => b.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("Clientes");
            e.HasKey(c => c.Id);
            e.Property(c => c.Identificacion).IsRequired().HasMaxLength(30);
            e.Property(c => c.NombreCompleto).IsRequired().HasMaxLength(150);
            e.Property(c => c.Correo).IsRequired().HasMaxLength(150);
            e.Property(c => c.Telefono).IsRequired().HasMaxLength(25);
            e.Property(c => c.Direccion).IsRequired().HasMaxLength(250);
            e.HasIndex(c => c.Identificacion).IsUnique();
        });

        modelBuilder.Entity<SolicitudCredito>(e =>
        {
            e.ToTable("SolicitudesCredito");
            e.HasKey(s => s.Id);
            e.Property(s => s.Monto).HasPrecision(18, 2).IsRequired();
            e.Property(s => s.Comentarios).HasMaxLength(1000);
            e.Property(s => s.Estado).IsRequired().HasMaxLength(30);
            e.HasIndex(s => new { s.ClienteId, s.Estado });
            e.HasOne<Cliente>().WithMany().HasForeignKey(s => s.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Usuario>().WithMany().HasForeignKey(s => s.UsuarioRegistroId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentoSolicitudCredito>(e =>
        {
            e.ToTable("DocumentosSolicitudCredito");
            e.HasKey(d => d.Id);
            e.Property(d => d.NombreOriginal).IsRequired().HasMaxLength(255);
            e.Property(d => d.NombreAlmacenado).IsRequired().HasMaxLength(100);
            e.Property(d => d.RutaRelativa).IsRequired().HasMaxLength(500);
            e.HasOne(d => d.Solicitud).WithMany(s => s.Documentos)
                .HasForeignKey(d => d.SolicitudCreditoId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
