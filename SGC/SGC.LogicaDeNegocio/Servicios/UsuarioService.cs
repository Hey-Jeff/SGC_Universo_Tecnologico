using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;
using NombresRol = SGC.Abstracciones.Constantes.Roles;

namespace SGC.LogicaDeNegocio.Servicios;

public class UsuarioService : IUsuarioService
{
    private const int LongitudMinimaPassword = 8;
    private const string EntidadBitacora = "Usuario";

    private readonly IUsuarioRepositorio _usuarios;
    private readonly IRolRepositorio _roles;
    private readonly IPasswordService _passwords;
    private readonly IBitacoraService _bitacora;

    public UsuarioService(
        IUsuarioRepositorio usuarios,
        IRolRepositorio roles,
        IPasswordService passwords,
        IBitacoraService bitacora)
    {
        _usuarios = usuarios;
        _roles = roles;
        _passwords = passwords;
        _bitacora = bitacora;
    }

    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync()
    {
        var usuarios = await _usuarios.ObtenerTodosAsync();
        return usuarios.Select(u => u.ADto()).ToList();
    }

    public async Task<UsuarioDto?> ObtenerAsync(int id)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(id);
        return usuario?.ADto();
    }

    public async Task<IReadOnlyList<RolDto>> ListarRolesAsync()
    {
        var roles = await _roles.ObtenerTodosAsync();
        return roles.Select(r => r.ADto()).ToList();
    }

    public async Task<ResultadoOperacion> CrearAsync(UsuarioCrearDto dto, int usuarioActualId)
    {
        var correo = UsuarioMapeo.NormalizarCorreo(dto.Correo);

        if (await _usuarios.ExisteCorreoAsync(correo))
        {
            return ResultadoOperacion.Fallo("Ya existe un usuario con ese correo.");
        }

        var roles = await ObtenerRolesValidosAsync(dto.RolIds);
        if (roles is null)
        {
            return ResultadoOperacion.Fallo("Seleccione al menos un rol válido.");
        }

        var errorPassword = ValidarPassword(dto.Password);
        if (errorPassword is not null)
        {
            return ResultadoOperacion.Fallo(errorPassword);
        }

        var usuario = new Usuario
        {
            Nombre = dto.Nombre.Trim(),
            Correo = correo,
            PasswordHash = _passwords.Hash(dto.Password),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        foreach (var rol in roles)
        {
            usuario.UsuarioRoles.Add(new UsuarioRol { RolId = rol.Id });
        }

        await _usuarios.AgregarAsync(usuario);

        await _bitacora.RegistrarAsync(
            usuarioActualId,
            EntidadBitacora,
            usuario.Correo,
            "Crear",
            $"Se crea el usuario con roles: {NombresDeRoles(roles)}.");

        return ResultadoOperacion.Ok("Usuario creado correctamente.");
    }

    public async Task<ResultadoOperacion> EditarAsync(UsuarioEditarDto dto, int usuarioActualId)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(dto.Id);
        if (usuario is null)
        {
            return ResultadoOperacion.Fallo("El usuario no existe.");
        }

        var correo = UsuarioMapeo.NormalizarCorreo(dto.Correo);
        if (await _usuarios.ExisteCorreoAsync(correo, dto.Id))
        {
            return ResultadoOperacion.Fallo("Ya existe otro usuario con ese correo.");
        }

        var rolesNuevos = await ObtenerRolesValidosAsync(dto.RolIds);
        if (rolesNuevos is null)
        {
            return ResultadoOperacion.Fallo("Seleccione al menos un rol válido.");
        }

        var cambiaPassword = !string.IsNullOrWhiteSpace(dto.Password);
        if (cambiaPassword)
        {
            var errorPassword = ValidarPassword(dto.Password!);
            if (errorPassword is not null)
            {
                return ResultadoOperacion.Fallo(errorPassword);
            }
        }

        // Siempre debe quedar al menos un administrador activo.
        var eraAdminActivo = usuario.Activo && EsAdministrador(usuario);
        var seraAdminActivo = dto.Activo && rolesNuevos.Any(r => r.Nombre == NombresRol.Administrador);
        if (eraAdminActivo && !seraAdminActivo && await _usuarios.ContarAdministradoresActivosAsync() <= 1)
        {
            return ResultadoOperacion.Fallo("No se puede quitar el rol ni desactivar al último administrador activo.");
        }

        var rolesAnteriores = string.Join(", ", usuario.UsuarioRoles.Select(ur => ur.Rol.Descripcion));

        usuario.Nombre = dto.Nombre.Trim();
        usuario.Correo = correo;
        usuario.Activo = dto.Activo;
        if (cambiaPassword)
        {
            usuario.PasswordHash = _passwords.Hash(dto.Password!);
        }

        ActualizarRoles(usuario, rolesNuevos);

        await _usuarios.ActualizarAsync(usuario);

        var detalle = $"Roles: [{rolesAnteriores}] -> [{NombresDeRoles(rolesNuevos)}]. " +
                      $"Estado: {(usuario.Activo ? "activo" : "inactivo")}." +
                      (cambiaPassword ? " Se cambió la contraseña." : string.Empty);

        await _bitacora.RegistrarAsync(usuarioActualId, EntidadBitacora, usuario.Correo, "Editar", detalle);

        return ResultadoOperacion.Ok("Usuario actualizado correctamente.");
    }

    public async Task<ResultadoOperacion> EliminarAsync(int id, int usuarioActualId)
    {
        if (id == usuarioActualId)
        {
            return ResultadoOperacion.Fallo("No puede eliminar su propio usuario.");
        }

        var usuario = await _usuarios.ObtenerPorIdAsync(id);
        if (usuario is null)
        {
            return ResultadoOperacion.Fallo("El usuario no existe.");
        }

        if (!usuario.Activo)
        {
            return ResultadoOperacion.Fallo("El usuario ya está eliminado (inactivo).");
        }

        if (EsAdministrador(usuario) && await _usuarios.ContarAdministradoresActivosAsync() <= 1)
        {
            return ResultadoOperacion.Fallo("No se puede eliminar al último administrador activo.");
        }

        // Borrado lógico: se conserva el registro para no perder el historial ni la bitácora.
        usuario.Activo = false;
        await _usuarios.ActualizarAsync(usuario);

        await _bitacora.RegistrarAsync(
            usuarioActualId,
            EntidadBitacora,
            usuario.Correo,
            "Eliminar",
            "Se inactiva el usuario (borrado lógico).");

        return ResultadoOperacion.Ok("Usuario eliminado correctamente.");
    }

    private async Task<IReadOnlyList<Rol>?> ObtenerRolesValidosAsync(IReadOnlyCollection<int> ids)
    {
        var idsUnicos = ids.Distinct().ToList();
        if (idsUnicos.Count == 0)
        {
            return null;
        }

        var roles = await _roles.ObtenerPorIdsAsync(idsUnicos);
        return roles.Count == idsUnicos.Count ? roles : null;
    }

    private static void ActualizarRoles(Usuario usuario, IReadOnlyList<Rol> rolesNuevos)
    {
        var idsNuevos = rolesNuevos.Select(r => r.Id).ToHashSet();

        var porQuitar = usuario.UsuarioRoles.Where(ur => !idsNuevos.Contains(ur.RolId)).ToList();
        foreach (var ur in porQuitar)
        {
            usuario.UsuarioRoles.Remove(ur);
        }

        var existentes = usuario.UsuarioRoles.Select(ur => ur.RolId).ToHashSet();
        foreach (var rol in rolesNuevos.Where(r => !existentes.Contains(r.Id)))
        {
            usuario.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuario.Id, RolId = rol.Id });
        }
    }

    private static bool EsAdministrador(Usuario usuario) =>
        usuario.UsuarioRoles.Any(ur => ur.Rol.Nombre == NombresRol.Administrador);

    private static string NombresDeRoles(IEnumerable<Rol> roles) =>
        string.Join(", ", roles.Select(r => r.Descripcion));

    private static string? ValidarPassword(string password)
    {
        if (password.Length < LongitudMinimaPassword)
        {
            return $"La contraseña debe tener al menos {LongitudMinimaPassword} caracteres.";
        }

        if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
        {
            return "La contraseña debe incluir mayúsculas, minúsculas y números.";
        }

        return null;
    }
}
