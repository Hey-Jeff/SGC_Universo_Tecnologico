using SGC.Abstracciones.Constantes;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;

namespace SGC.LogicaDeNegocio.Servicios;

public class InicializadorService : IInicializadorService
{
    // Credenciales del primer ingreso
    private const string CorreoAdminInicial = "admin@sgc.com";
    private const string PasswordAdminInicial = "Admin123*";

    private readonly IUsuarioRepositorio _usuarios;
    private readonly IRolRepositorio _roles;
    private readonly IPasswordService _passwords;

    public InicializadorService(IUsuarioRepositorio usuarios, IRolRepositorio roles, IPasswordService passwords)
    {
        _usuarios = usuarios;
        _roles = roles;
        _passwords = passwords;
    }

    public async Task InicializarAsync()
    {
        if (await _usuarios.ExisteAlgunoAsync())
        {
            return;
        }

        var rolAdmin = await _roles.ObtenerPorNombreAsync(Roles.Administrador)
            ?? throw new InvalidOperationException("No se encontró el rol Administrador en la base de datos.");

        await _usuarios.AgregarAsync(new Usuario
        {
            Nombre = "Administrador",
            Correo = CorreoAdminInicial,
            PasswordHash = _passwords.Hash(PasswordAdminInicial),
            RolId = rolAdmin.Id,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        });
    }
}
