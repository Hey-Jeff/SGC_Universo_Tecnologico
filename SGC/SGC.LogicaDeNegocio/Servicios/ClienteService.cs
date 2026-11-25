using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;

namespace SGC.LogicaDeNegocio.Servicios;

public class ClienteService : IClienteService
{
    private const string EntidadBitacora = "Cliente";

    private readonly IClienteRepositorio _clientes;
    private readonly IBitacoraService _bitacora;

    public ClienteService(IClienteRepositorio clientes, IBitacoraService bitacora)
    {
        _clientes = clientes;
        _bitacora = bitacora;
    }

    public async Task<IReadOnlyList<ClienteDto>> ListarAsync(string? filtro = null)
    {
        var lista = await _clientes.ObtenerTodosAsync(filtro);
        return lista.Select(ADto).ToList();
    }

    public async Task<ClienteDto?> ObtenerAsync(int id)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(id);
        return cliente is null ? null : ADto(cliente);
    }

    public async Task<ResultadoOperacion> CrearAsync(ClienteCrearDto dto, int usuarioActualId)
    {
        var identificacion = NormalizarIdentificacion(dto.Identificacion);
        var correo = NormalizarCorreo(dto.Correo);

        if (string.IsNullOrWhiteSpace(identificacion))
        {
            return ResultadoOperacion.Fallo("La identificación es obligatoria.");
        }

        if (await _clientes.ExisteIdentificacionAsync(identificacion))
        {
            return ResultadoOperacion.Fallo($"Ya existe un cliente registrado con la identificación {identificacion}.");
        }

        if (await _clientes.ExisteCorreoAsync(correo))
        {
            return ResultadoOperacion.Fallo("Ya existe un cliente registrado con ese correo electrónico.");
        }

        var cliente = new Cliente
        {
            Identificacion = identificacion,
            NombreCompleto = dto.NombreCompleto.Trim(),
            Correo = correo,
            Telefono = dto.Telefono.Trim(),
            Direccion = dto.Direccion.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _clientes.AgregarAsync(cliente);

        await _bitacora.RegistrarAsync(
            usuarioActualId,
            EntidadBitacora,
            cliente.Identificacion,
            "Crear",
            $"Se crea el cliente {cliente.NombreCompleto} ({cliente.Identificacion}).");

        return ResultadoOperacion.Ok("Cliente creado correctamente.");
    }

    public async Task<ResultadoOperacion> EditarAsync(ClienteEditarDto dto, int usuarioActualId)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(dto.Id);
        if (cliente is null)
        {
            return ResultadoOperacion.Fallo("El cliente no existe.");
        }

        var identificacion = NormalizarIdentificacion(dto.Identificacion);
        var correo = NormalizarCorreo(dto.Correo);

        if (await _clientes.ExisteIdentificacionAsync(identificacion, dto.Id))
        {
            return ResultadoOperacion.Fallo($"Ya existe otro cliente con la identificación {identificacion}.");
        }

        if (await _clientes.ExisteCorreoAsync(correo, dto.Id))
        {
            return ResultadoOperacion.Fallo("Ya existe otro cliente con ese correo electrónico.");
        }

        cliente.Identificacion = identificacion;
        cliente.NombreCompleto = dto.NombreCompleto.Trim();
        cliente.Correo = correo;
        cliente.Telefono = dto.Telefono.Trim();
        cliente.Direccion = dto.Direccion.Trim();
        cliente.Activo = dto.Activo;

        await _clientes.ActualizarAsync(cliente);

        await _bitacora.RegistrarAsync(
            usuarioActualId,
            EntidadBitacora,
            cliente.Identificacion,
            "Editar",
            $"Se actualiza el cliente {cliente.NombreCompleto} ({cliente.Identificacion}). Estado: {(cliente.Activo ? "activo" : "inactivo")}.");

        return ResultadoOperacion.Ok("Cliente actualizado correctamente.");
    }

    public async Task<ResultadoOperacion> EliminarAsync(int id, int usuarioActualId)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(id);
        if (cliente is null)
        {
            return ResultadoOperacion.Fallo("El cliente no existe.");
        }

        if (!cliente.Activo)
        {
            return ResultadoOperacion.Fallo("El cliente ya se encuentra inactivo.");
        }

        cliente.Activo = false;
        await _clientes.ActualizarAsync(cliente);

        await _bitacora.RegistrarAsync(
            usuarioActualId,
            EntidadBitacora,
            cliente.Identificacion,
            "Eliminar",
            $"Se inactiva el cliente {cliente.NombreCompleto} (borrado lógico).");

        return ResultadoOperacion.Ok("Cliente eliminado correctamente.");
    }

    private static ClienteDto ADto(Cliente c) => new()
    {
        Id = c.Id,
        Identificacion = c.Identificacion,
        NombreCompleto = c.NombreCompleto,
        Correo = c.Correo,
        Telefono = c.Telefono,
        Direccion = c.Direccion,
        Activo = c.Activo,
        FechaCreacion = c.FechaCreacion
    };

    private static string NormalizarIdentificacion(string identificacion) => identificacion.Trim().ToUpperInvariant();
    private static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();
}