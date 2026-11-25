using SGC.Abstracciones.Entidades;

namespace SGC.Abstracciones.Interfaces;

public interface IClienteRepositorio
{
    Task<IReadOnlyList<Cliente>> ObtenerTodosAsync(string? filtro = null);
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<Cliente?> ObtenerPorIdentificacionAsync(string identificacion);
    Task<bool> ExisteIdentificacionAsync(string identificacion, int? excluirId = null);
    Task<bool> ExisteCorreoAsync(string correo, int? excluirId = null);
    Task AgregarAsync(Cliente cliente);
    Task ActualizarAsync(Cliente cliente);
}