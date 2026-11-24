using Microsoft.EntityFrameworkCore;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;

namespace SGC.AccesoADatos.Repositorios;

public class RolRepositorio : IRolRepositorio
{
    private readonly SgcDbContext _db;

    public RolRepositorio(SgcDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Rol>> ObtenerTodosAsync() =>
        await _db.Roles.AsNoTracking().OrderBy(r => r.Id).ToListAsync();

    public Task<Rol?> ObtenerPorNombreAsync(string nombre) =>
        _db.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre);

    public async Task<IReadOnlyList<Rol>> ObtenerPorIdsAsync(IReadOnlyCollection<int> ids) =>
        await _db.Roles.Where(r => ids.Contains(r.Id)).ToListAsync();
}
