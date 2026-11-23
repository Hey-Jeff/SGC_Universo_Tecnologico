using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Interfaces;

namespace SGC.LogicaDeNegocio.Servicios;

public class AuthService : IAuthService
{
    private const string CredencialesInvalidas = "Correo o contraseña incorrectos.";

    private readonly IUsuarioRepositorio _usuarios;
    private readonly IPasswordService _passwords;

    public AuthService(IUsuarioRepositorio usuarios, IPasswordService passwords)
    {
        _usuarios = usuarios;
        _passwords = passwords;
    }

    public async Task<ResultadoOperacion<UsuarioDto>> AutenticarAsync(LoginDto dto)
    {
        var usuario = await _usuarios.ObtenerPorCorreoAsync(UsuarioMapeo.NormalizarCorreo(dto.Correo));

        // Mismo mensaje para correo inexistente y contraseña 
        if (usuario is null || !_passwords.Verificar(dto.Password, usuario.PasswordHash))
        {
            return ResultadoOperacion<UsuarioDto>.Fallo(CredencialesInvalidas);
        }

        if (!usuario.Activo)
        {
            return ResultadoOperacion<UsuarioDto>.Fallo("El usuario está inactivo. Contacte al administrador.");
        }

        return ResultadoOperacion<UsuarioDto>.Ok(usuario.ADto());
    }
}
