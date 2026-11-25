using Microsoft.EntityFrameworkCore;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;

namespace SGC.AccesoADatos.Repositorios;

public class ClienteRepositorio : IClienteRepositorio
{
    private readonly SgcDbContext _db;

    public ClienteRepositorio(SgcDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Cliente>> ObtenerTodosAsync(string? filtro = null)
    {
        var consulta = _db.Clientes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var texto = filtro.Trim().ToLower();
            consulta = consulta.Where(c =>
                c.Identificacion.ToLower().Contains(texto) ||
                c.NombreCompleto.ToLower().Contains(texto) ||
                c.Correo.ToLower().Contains(texto));
        }

        return await consulta.OrderBy(c => c.NombreCompleto).ToListAsync();
    }

    public Task<Cliente?> ObtenerPorIdAsync(int id) =>
        _db.Clientes.FirstOrDefaultAsync(c => c.Id == id);

    public Task<Cliente?> ObtenerPorIdentificacionAsync(string identificacion)
    {
        var normalizada = identificacion.Trim().ToUpperInvariant();
        return _db.Clientes.FirstOrDefaultAsync(c => c.Identificacion == normalizada);
    }

    public Task<bool> ExisteIdentificacionAsync(string identificacion, int? excluirId = null)
    {
        var normalizada = identificacion.Trim().ToUpperInvariant();
        return _db.Clientes.AnyAsync(c =>
            c.Identificacion == normalizada && (excluirId == null || c.Id != excluirId));
    }

    public Task<bool> ExisteCorreoAsync(string correo, int? excluirId = null)
    {
        var normalizado = correo.Trim().ToLowerInvariant();
        return _db.Clientes.AnyAsync(c =>
            c.Correo == normalizado && (excluirId == null || c.Id != excluirId));
    }

    public async Task AgregarAsync(Cliente cliente)
    {
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Cliente cliente)
    {
        await _db.SaveChangesAsync();
    }
}