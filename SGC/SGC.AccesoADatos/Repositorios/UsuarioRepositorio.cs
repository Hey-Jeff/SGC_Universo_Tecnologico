using Microsoft.EntityFrameworkCore;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;
using NombresRol = SGC.Abstracciones.Constantes.Roles;

namespace SGC.AccesoADatos.Repositorios;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly SgcDbContext _db;

    public UsuarioRepositorio(SgcDbContext db)
    {
        _db = db;
    }

    private IQueryable<Usuario> ConRoles() =>
        _db.Usuarios.Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol);

    public async Task<IReadOnlyList<Usuario>> ObtenerTodosAsync() =>
        await ConRoles().AsNoTracking().OrderBy(u => u.Nombre).ToListAsync();

    public Task<Usuario?> ObtenerPorIdAsync(int id) =>
        ConRoles().FirstOrDefaultAsync(u => u.Id == id);

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo) =>
        ConRoles().FirstOrDefaultAsync(u => u.Correo == correo);

    public Task<bool> ExisteCorreoAsync(string correo, int? excluirId = null) =>
        _db.Usuarios.AnyAsync(u => u.Correo == correo && (excluirId == null || u.Id != excluirId));

    public Task<bool> ExisteAlgunoAsync() => _db.Usuarios.AnyAsync();

    public Task<int> ContarAdministradoresActivosAsync() =>
        _db.Usuarios.CountAsync(u => u.Activo && u.UsuarioRoles.Any(ur => ur.Rol.Nombre == NombresRol.Administrador));

    public async Task AgregarAsync(Usuario usuario)
    {
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        // La entidad viene de ObtenerPorIdAsync (rastreada), basta con guardar los cambios.
        await _db.SaveChangesAsync();
    }
}
