using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Entidades;

namespace SGC.Abstracciones.Interfaces;

public interface ISeguimientoRepositorio
{
    Task<IReadOnlyList<CreditoFila>> ListarAsync(string[] estados, bool todos);
    Task<CreditoDetalle?> ObtenerAsync(int id, string[] estados, bool todos);
    Task<IReadOnlyList<TrackingFila>> ReporteAsync(string[] estados, bool todos, int? gestion);
    Task<ArchivoCredito?> DescargarAsync(int id, string[] estados, bool todos);
    Task<ResultadoOperacion> CambiarAsync(int id, string version, string estadoAnterior,
        string estadoNuevo, string accion, string comentario, int usuario,
        IReadOnlyList<DocumentoCredito> documentos);
}

public interface ISeguimientoService
{
    Task<IReadOnlyList<CreditoFila>> ListarAsync(PermisosCredito permisos);
    Task<SeguimientoVista?> ObtenerAsync(int id, PermisosCredito permisos);
    Task<IReadOnlyList<TrackingFila>> ReporteAsync(PermisosCredito permisos, int? gestion);
    Task<ArchivoCredito?> DescargarAsync(int id, PermisosCredito permisos);
    Task<ResultadoOperacion> CambiarAsync(MovimientoCrearDto dto,
        IReadOnlyList<DocumentoEntrada> documentos, int usuario, PermisosCredito permisos);
}
