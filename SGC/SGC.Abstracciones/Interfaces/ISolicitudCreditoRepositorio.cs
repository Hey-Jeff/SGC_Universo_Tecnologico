using SGC.Abstracciones.Entidades;

namespace SGC.Abstracciones.Interfaces;

public interface ISolicitudCreditoRepositorio
{
    Task<IReadOnlyList<SolicitudCredito>> ListarAsync();
    Task<bool> TieneSolicitudActivaAsync(int clienteId);
    Task AgregarAsync(SolicitudCredito solicitud);
}
