using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SGC.Abstracciones.Interfaces;
using SGC.AccesoADatos.Repositorios;

namespace SGC.AccesoADatos;

public static class InyeccionDependencias
{
    public static IServiceCollection AddAccesoADatos(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SgcDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.AddScoped<IRolRepositorio, RolRepositorio>();
        services.AddScoped<IBitacoraRepositorio, BitacoraRepositorio>();

        services.AddScoped<IClienteRepositorio, ClienteRepositorio>();

        return services;
    }

    /// <summary>
    /// Crea o actualiza la base de datos. Usa migraciones si el equipo ya las generó;
    /// si no, crea el esquema directamente con EnsureCreated.
    /// </summary>
    public static async Task InicializarBaseDeDatosAsync(this IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SgcDbContext>();

        if (db.Database.GetMigrations().Any())
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }
    }
}
