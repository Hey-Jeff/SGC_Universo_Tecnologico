using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Entidades;

namespace SGC.LogicaDeNegocio.Servicios;

internal static class UsuarioMapeo
{
    public static UsuarioDto ADto(this Usuario u) => new()
    {
        Id = u.Id,
        Nombre = u.Nombre,
        Correo = u.Correo,
        RolId = u.RolId,
        RolNombre = u.Rol?.Nombre ?? string.Empty,
        RolDescripcion = u.Rol?.Descripcion ?? string.Empty,
        Activo = u.Activo,
        FechaCreacion = u.FechaCreacion
    };

    public static RolDto ADto(this Rol r) => new()
    {
        Id = r.Id,
        Nombre = r.Nombre,
        Descripcion = r.Descripcion
    };

    public static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();
}
