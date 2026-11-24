using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;
using NombresRol = SGC.Abstracciones.Constantes.Roles;

namespace SGC.LogicaDeNegocio.Servicios;

/// <summary>
/// Crea el primer administrador SOLO si hay credenciales configuradas (AdminInicial:Correo y AdminInicial:Password).
/// La contraseña nunca va en el código ni en el repositorio: se define con user-secrets o una variable de entorno.
/// </summary>
public class InicializadorService : IInicializadorService
{
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IRolRepositorio _roles;
    private readonly IPasswordService _passwords;
    private readonly IConfiguration _configuracion;
    private readonly ILogger<InicializadorService> _log;

    public InicializadorService(
        IUsuarioRepositorio usuarios,
        IRolRepositorio roles,
        IPasswordService passwords,
        IConfiguration configuracion,
        ILogger<InicializadorService> log)
    {
        _usuarios = usuarios;
        _roles = roles;
        _passwords = passwords;
        _configuracion = configuracion;
        _log = log;
    }

    public async Task InicializarAsync()
    {
        if (await _usuarios.ExisteAlgunoAsync())
        {
            return;
        }

        var correo = _configuracion["AdminInicial:Correo"];
        var password = _configuracion["AdminInicial:Password"];

        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
        {
            _log.LogWarning(
                "No hay usuarios y falta la configuración AdminInicial:Correo / AdminInicial:Password. " +
                "Defínalas con 'dotnet user-secrets' (ver README) para crear el administrador inicial.");
            return;
        }

        var rolAdmin = await _roles.ObtenerPorNombreAsync(NombresRol.Administrador)
            ?? throw new InvalidOperationException("No se encontró el rol Administrador en la base de datos.");

        var admin = new Usuario
        {
            Nombre = "Administrador",
            Correo = UsuarioMapeo.NormalizarCorreo(correo),
            PasswordHash = _passwords.Hash(password),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
        admin.UsuarioRoles.Add(new UsuarioRol { RolId = rolAdmin.Id });

        await _usuarios.AgregarAsync(admin);
    }
}
