using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Entidades;

namespace SGC.Abstracciones.Interfaces;

public interface IUsuarioRepositorio
{
    Task<IReadOnlyList<Usuario>> ObtenerTodosAsync();
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);
    Task<bool> ExisteCorreoAsync(string correo, int? excluirId = null);
    Task<bool> ExisteAlgunoAsync();
    Task<int> ContarAdministradoresActivosAsync();
    Task AgregarAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);
}

public interface IRolRepositorio
{
    Task<IReadOnlyList<Rol>> ObtenerTodosAsync();
    Task<Rol?> ObtenerPorNombreAsync(string nombre);
    Task<IReadOnlyList<Rol>> ObtenerPorIdsAsync(IReadOnlyCollection<int> ids);
}

public interface IBitacoraRepositorio
{
    Task AgregarAsync(BitacoraEvento evento);
    Task<IReadOnlyList<BitacoraEvento>> ConsultarAsync(BitacoraFiltroDto filtro, int maximo);
}
