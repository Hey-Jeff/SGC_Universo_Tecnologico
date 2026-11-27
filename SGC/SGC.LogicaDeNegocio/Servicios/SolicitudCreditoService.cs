using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;

namespace SGC.LogicaDeNegocio.Servicios;

public class SolicitudCreditoService : ISolicitudCreditoService
{
    private const decimal MontoMaximo = 10_000_000m;
    private readonly IClienteRepositorio _clientes;
    private readonly ISolicitudCreditoRepositorio _solicitudes;
    private readonly IBitacoraService _bitacora;

    public SolicitudCreditoService(IClienteRepositorio clientes, ISolicitudCreditoRepositorio solicitudes, IBitacoraService bitacora)
    {
        _clientes = clientes;
        _solicitudes = solicitudes;
        _bitacora = bitacora;
    }

    public async Task<IReadOnlyList<SolicitudCreditoDto>> ListarAsync()
    {
        var solicitudes = await _solicitudes.ListarAsync();
        var resultado = new List<SolicitudCreditoDto>();
        foreach (var solicitud in solicitudes)
        {
            var cliente = await _clientes.ObtenerPorIdAsync(solicitud.ClienteId);
            resultado.Add(new SolicitudCreditoDto
            {
                Id = solicitud.Id,
                IdentificacionCliente = cliente?.Identificacion ?? "No disponible",
                NombreCliente = cliente?.NombreCompleto ?? "Cliente no disponible",
                Monto = solicitud.Monto,
                Comentarios = solicitud.Comentarios,
                Estado = solicitud.Estado,
                FechaRegistro = solicitud.FechaRegistro,
                CantidadDocumentos = solicitud.Documentos.Count
            });
        }
        return resultado;
    }

    public async Task<ResultadoOperacion> CrearAsync(SolicitudCreditoCrearDto dto, int usuarioActualId)
    {
        var identificacion = (dto.IdentificacionCliente ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(identificacion))
            return ResultadoOperacion.Fallo("Debe ingresar la identificación del cliente.");
        if (dto.Monto <= 0)
            return ResultadoOperacion.Fallo("El monto solicitado debe ser mayor que ₡0.");
        if (dto.Monto > MontoMaximo)
            return ResultadoOperacion.Fallo("No se puede registrar la solicitud: el monto máximo permitido es ₡10.000.000.");

        var cliente = await _clientes.ObtenerPorIdentificacionAsync(identificacion);
        if (cliente is null)
            return ResultadoOperacion.Fallo("No existe un cliente registrado con esa identificación.");
        if (!cliente.Activo)
            return ResultadoOperacion.Fallo("No se puede crear la solicitud porque el cliente se encuentra inactivo.");
        if (await _solicitudes.TieneSolicitudActivaAsync(cliente.Id))
            return ResultadoOperacion.Fallo("El cliente ya tiene una solicitud en estado Registrado o Devolución. No es posible registrar una nueva solicitud.");

        var solicitud = new SolicitudCredito
        {
            ClienteId = cliente.Id,
            Monto = decimal.Round(dto.Monto, 2),
            Comentarios = (dto.Comentarios ?? string.Empty).Trim(),
            Estado = EstadosSolicitudCredito.Registrado,
            FechaRegistro = DateTime.UtcNow,
            UsuarioRegistroId = usuarioActualId,
            Documentos = dto.Documentos.Select(d => new DocumentoSolicitudCredito
            {
                NombreOriginal = d.NombreOriginal,
                NombreAlmacenado = d.NombreAlmacenado,
                RutaRelativa = d.RutaRelativa,
                FechaCarga = DateTime.UtcNow
            }).ToList()
        };

        await _solicitudes.AgregarAsync(solicitud);
        await _bitacora.RegistrarAsync(usuarioActualId, "SolicitudCredito", solicitud.Id.ToString(), "Crear",
            $"Solicitud de crédito registrada para el cliente {cliente.Identificacion}. Monto: ₡{solicitud.Monto:N2}. Estado: {solicitud.Estado}.");
        return ResultadoOperacion.Ok($"Solicitud de crédito #{solicitud.Id} registrada correctamente.");
    }
}
