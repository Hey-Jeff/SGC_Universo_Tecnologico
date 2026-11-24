using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Entidades;

namespace SGC.LogicaDeNegocio.Servicios;

internal static class UsuarioMapeo
{
    public static UsuarioDto ADto(this Usuario u)
    {
        var roles = u.UsuarioRoles.Select(ur => ur.Rol).OrderBy(r => r.Id).ToList();

        return new UsuarioDto
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Correo = u.Correo,
            RolIds = roles.Select(r => r.Id).ToList(),
            RolNombres = roles.Select(r => r.Nombre).ToList(),
            RolesDescripcion = string.Join(", ", roles.Select(r => r.Descripcion)),
            Activo = u.Activo,
            FechaCreacion = u.FechaCreacion
        };
    }

    public static RolDto ADto(this Rol r) => new()
    {
        Id = r.Id,
        Nombre = r.Nombre,
        Descripcion = r.Descripcion
    };

    public static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();
}
