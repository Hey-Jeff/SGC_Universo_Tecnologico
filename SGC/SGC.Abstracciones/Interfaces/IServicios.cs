using SGC.Abstracciones.DTOs;

namespace SGC.Abstracciones.Interfaces;

public interface IPasswordService
{
    string Hash(string password);
    bool Verificar(string password, string hash);
}

public interface IAuthService
{
    Task<ResultadoOperacion<UsuarioDto>> AutenticarAsync(LoginDto dto);
}

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioDto>> ListarAsync();
    Task<UsuarioDto?> ObtenerAsync(int id);
    Task<IReadOnlyList<RolDto>> ListarRolesAsync();
    Task<ResultadoOperacion> CrearAsync(UsuarioCrearDto dto);
    Task<ResultadoOperacion> EditarAsync(UsuarioEditarDto dto);
    Task<ResultadoOperacion> EliminarAsync(int id, int usuarioActualId);
}


public interface IInicializadorService
{
    Task InicializarAsync();
}
