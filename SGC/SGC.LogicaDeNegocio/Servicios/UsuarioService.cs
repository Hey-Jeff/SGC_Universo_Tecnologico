using SGC.Abstracciones.Constantes;
using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;

namespace SGC.LogicaDeNegocio.Servicios;

public class UsuarioService : IUsuarioService
{
    private const int LongitudMinimaPassword = 8;

    private readonly IUsuarioRepositorio _usuarios;
    private readonly IRolRepositorio _roles;
    private readonly IPasswordService _passwords;

    public UsuarioService(IUsuarioRepositorio usuarios, IRolRepositorio roles, IPasswordService passwords)
    {
        _usuarios = usuarios;
        _roles = roles;
        _passwords = passwords;
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

    public async Task<ResultadoOperacion> CrearAsync(UsuarioCrearDto dto)
    {
        var correo = UsuarioMapeo.NormalizarCorreo(dto.Correo);

        if (await _usuarios.ExisteCorreoAsync(correo))
        {
            return ResultadoOperacion.Fallo("Ya existe un usuario con ese correo.");
        }

        var rol = await _roles.ObtenerPorIdAsync(dto.RolId);
        if (rol is null)
        {
            return ResultadoOperacion.Fallo("El rol seleccionado no existe.");
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
            RolId = rol.Id,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _usuarios.AgregarAsync(usuario);
        return ResultadoOperacion.Ok("Usuario creado correctamente.");
    }

    public async Task<ResultadoOperacion> EditarAsync(UsuarioEditarDto dto)
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

        var rolNuevo = await _roles.ObtenerPorIdAsync(dto.RolId);
        if (rolNuevo is null)
        {
            return ResultadoOperacion.Fallo("El rol seleccionado no existe.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var errorPassword = ValidarPassword(dto.Password);
            if (errorPassword is not null)
            {
                return ResultadoOperacion.Fallo(errorPassword);
            }

            usuario.PasswordHash = _passwords.Hash(dto.Password);
        }

        // Siempre debe quedar al menos un administrador activo
        var eraAdminActivo = usuario.Activo && usuario.Rol.Nombre == Roles.Administrador;
        var seraAdminActivo = dto.Activo && rolNuevo.Nombre == Roles.Administrador;
        if (eraAdminActivo && !seraAdminActivo && await _usuarios.ContarAdministradoresActivosAsync() <= 1)
        {
            return ResultadoOperacion.Fallo("No se puede cambiar el rol ni desactivar al último administrador activo.");
        }

        usuario.Nombre = dto.Nombre.Trim();
        usuario.Correo = correo;
        usuario.Rol = rolNuevo;
        usuario.RolId = rolNuevo.Id;
        usuario.Activo = dto.Activo;

        await _usuarios.ActualizarAsync(usuario);
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

        if (usuario.Rol.Nombre == Roles.Administrador && await _usuarios.ContarAdministradoresActivosAsync() <= 1)
        {
            return ResultadoOperacion.Fallo("No se puede eliminar al último administrador activo.");
        }

        //  conserva el registro para no romper el historial de gestiones
        usuario.Activo = false;
        await _usuarios.ActualizarAsync(usuario);

        return ResultadoOperacion.Ok("Usuario eliminado correctamente.");
    }

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
