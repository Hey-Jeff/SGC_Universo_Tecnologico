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
    Task<Rol?> ObtenerPorIdAsync(int id);
    Task<Rol?> ObtenerPorNombreAsync(string nombre);
}
