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
    Task<ResultadoOperacion> CrearAsync(UsuarioCrearDto dto, int usuarioActualId);
    Task<ResultadoOperacion> EditarAsync(UsuarioEditarDto dto, int usuarioActualId);
    Task<ResultadoOperacion> EliminarAsync(int id, int usuarioActualId);
}

public interface IBitacoraService
{
    /// <summary>Registra un evento de auditoría. Todos los módulos deben usarlo en sus acciones relevantes.</summary>
    Task RegistrarAsync(int usuarioId, string entidad, string registro, string accion, string? detalle = null);

    Task<IReadOnlyList<BitacoraEventoDto>> ConsultarAsync(BitacoraFiltroDto filtro);
}

/// <summary>Crea los datos mínimos al arrancar (usuario administrador inicial).</summary>
public interface IInicializadorService
{
    Task InicializarAsync();
}
