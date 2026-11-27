using System.ComponentModel.DataAnnotations;
using SGC.Abstracciones.Constantes;
using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Interfaces;

namespace SGC.LogicaDeNegocio.Servicios;

public class SeguimientoService(ISeguimientoRepositorio repositorio) : ISeguimientoService
{
    private static string[] Estados(PermisosCredito permisos)
    {
        var estados = new List<string>();
        if (permisos.Analista)
        {
            estados.Add(EstadosCredito.Registrado);
            estados.Add(EstadosCredito.Devolucion);
        }
        if (permisos.Gestor) estados.Add(EstadosCredito.Enviado);
        return estados.ToArray();
    }

    private static bool PuedeAnalizar(PermisosCredito p, string estado) =>
        (p.Analista || p.Administrador) && EstadosCredito.PendienteAnalisis(estado);

    private static bool PuedeDecidir(PermisosCredito p, string estado) =>
        (p.Gestor || p.Administrador) && estado == EstadosCredito.Enviado;

    public Task<IReadOnlyList<CreditoFila>> ListarAsync(PermisosCredito permisos) =>
        repositorio.ListarAsync(Estados(permisos), permisos.Administrador);

    public async Task<SeguimientoVista?> ObtenerAsync(int id, PermisosCredito permisos)
    {
        var detalle = await repositorio.ObtenerAsync(id, Estados(permisos), permisos.Administrador);
        return detalle is null ? null : new SeguimientoVista(detalle,
            PuedeAnalizar(permisos, detalle.Datos.Estado), PuedeDecidir(permisos, detalle.Datos.Estado));
    }

    public Task<IReadOnlyList<TrackingFila>> ReporteAsync(PermisosCredito permisos, int? gestion) =>
        repositorio.ReporteAsync(Estados(permisos), permisos.Administrador, gestion);

    public Task<ArchivoCredito?> DescargarAsync(int id, PermisosCredito permisos) =>
        repositorio.DescargarAsync(id, Estados(permisos), permisos.Administrador);

    public async Task<ResultadoOperacion> CambiarAsync(MovimientoCrearDto dto,
        IReadOnlyList<DocumentoEntrada> documentos, int usuario, PermisosCredito permisos)
    {
        if (usuario <= 0 || !Validator.TryValidateObject(dto, new ValidationContext(dto), null, true))
            return ResultadoOperacion.Fallo("Datos de modificación inválidos.");

        var vista = await ObtenerAsync(dto.Id, permisos);
        if (vista is null) return ResultadoOperacion.Fallo("Gestión no disponible para su rol.");
        var detalle = vista.Detalle;
        if (detalle.Version != dto.Version) return ResultadoOperacion.Fallo("La gestión cambió. Recargue.");

        var estado = detalle.Datos.Estado;
        var nuevo = estado;
        var comentario = dto.Comentario?.Trim() ?? string.Empty;
        string accion;
        switch (dto.Accion)
        {
            case "Adjuntar" when vista.PuedeAnalizar:
                if (documentos.Count == 0) return ResultadoOperacion.Fallo("Seleccione al menos un documento.");
                accion = "Adjuntar documentos";
                break;
            case "Enviar" when vista.PuedeAnalizar:
                nuevo = EstadosCredito.Enviado;
                accion = "Enviar a aprobación";
                break;
            case "Aprobar" when vista.PuedeDecidir:
                nuevo = EstadosCredito.Aprobado;
                accion = "Aprobar";
                break;
            case "Devolver" when vista.PuedeDecidir:
                nuevo = EstadosCredito.Devolucion;
                accion = "Devolver";
                break;
            default:
                return ResultadoOperacion.Fallo("Acción no permitida para su rol o estado actual.");
        }

        if (vista.PuedeDecidir && (comentario.Length == 0 || documentos.Count > 0))
            return ResultadoOperacion.Fallo("Ingrese un comentario y no adjunte documentos en esta acción.");
        if (ValidacionDocumentos.Error(documentos) is { } error)
            return ResultadoOperacion.Fallo(error);

        return await repositorio.CambiarAsync(dto.Id, dto.Version, estado, nuevo, accion,
            comentario, usuario, documentos.Select(d => ValidacionDocumentos.Convertir(d, usuario)).ToList());
    }
}
