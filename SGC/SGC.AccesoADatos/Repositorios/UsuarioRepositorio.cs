using Microsoft.EntityFrameworkCore;
using SGC.Abstracciones.Constantes;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;

namespace SGC.AccesoADatos.Repositorios;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly SgcDbContext _db;

    public UsuarioRepositorio(SgcDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Usuario>> ObtenerTodosAsync() =>
        await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .OrderBy(u => u.Nombre)
            .ToListAsync();

    public Task<Usuario?> ObtenerPorIdAsync(int id) =>
        _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo) =>
        _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Correo == correo);

    public Task<bool> ExisteCorreoAsync(string correo, int? excluirId = null) =>
        _db.Usuarios.AnyAsync(u => u.Correo == correo && (excluirId == null || u.Id != excluirId));

    public Task<bool> ExisteAlgunoAsync() => _db.Usuarios.AnyAsync();

    public Task<int> ContarAdministradoresActivosAsync() =>
        _db.Usuarios.CountAsync(u => u.Activo && u.Rol.Nombre == Roles.Administrador);

    public async Task AgregarAsync(Usuario usuario)
    {
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        
        await _db.SaveChangesAsync();
    }
}
