using SGC.Abstracciones.DTOs;

namespace SGC.Abstracciones.Interfaces;

public interface ISolicitudCreditoService
{
    Task<IReadOnlyList<SolicitudCreditoDto>> ListarAsync();
    Task<ResultadoOperacion> CrearAsync(SolicitudCreditoCrearDto dto, int usuarioActualId);
}
