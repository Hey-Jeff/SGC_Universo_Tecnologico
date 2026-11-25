using SGC.Abstracciones.DTOs;

namespace SGC.Abstracciones.Interfaces;

public interface IClienteService
{
    Task<IReadOnlyList<ClienteDto>> ListarAsync(string? filtro = null);
    Task<ClienteDto?> ObtenerAsync(int id);
    Task<ResultadoOperacion> CrearAsync(ClienteCrearDto dto, int usuarioActualId);
    Task<ResultadoOperacion> EditarAsync(ClienteEditarDto dto, int usuarioActualId);
    Task<ResultadoOperacion> EliminarAsync(int id, int usuarioActualId);
}