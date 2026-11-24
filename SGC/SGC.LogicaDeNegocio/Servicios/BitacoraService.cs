using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;

namespace SGC.LogicaDeNegocio.Servicios;

public class BitacoraService : IBitacoraService
{
    private const int MaximoRegistros = 500;

    private readonly IBitacoraRepositorio _bitacora;

    public BitacoraService(IBitacoraRepositorio bitacora)
    {
        _bitacora = bitacora;
    }

    public Task RegistrarAsync(int usuarioId, string entidad, string registro, string accion, string? detalle = null)
    {
        return _bitacora.AgregarAsync(new BitacoraEvento
        {
            UsuarioId = usuarioId,
            Entidad = entidad,
            Registro = registro,
            Accion = accion,
            Detalle = detalle,
            Fecha = DateTime.UtcNow
        });
    }

    public async Task<IReadOnlyList<BitacoraEventoDto>> ConsultarAsync(BitacoraFiltroDto filtro)
    {
        var eventos = await _bitacora.ConsultarAsync(filtro, MaximoRegistros);

        return eventos.Select(e => new BitacoraEventoDto
        {
            Id = e.Id,
            Fecha = e.Fecha,
            UsuarioNombre = e.Usuario.Nombre,
            Entidad = e.Entidad,
            Registro = e.Registro,
            Accion = e.Accion,
            Detalle = e.Detalle
        }).ToList();
    }
}
