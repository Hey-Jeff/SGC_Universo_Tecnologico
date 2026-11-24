using Microsoft.EntityFrameworkCore;
using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;

namespace SGC.AccesoADatos.Repositorios;

public class BitacoraRepositorio : IBitacoraRepositorio
{
    private readonly SgcDbContext _db;

    public BitacoraRepositorio(SgcDbContext db)
    {
        _db = db;
    }

    public async Task AgregarAsync(BitacoraEvento evento)
    {
        _db.Bitacora.Add(evento);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<BitacoraEvento>> ConsultarAsync(BitacoraFiltroDto filtro, int maximo)
    {
        var consulta = _db.Bitacora.AsNoTracking().Include(b => b.Usuario).AsQueryable();

        if (filtro.Desde.HasValue)
        {
            consulta = consulta.Where(b => b.Fecha >= filtro.Desde.Value);
        }

        if (filtro.Hasta.HasValue)
        {
            // "Hasta" incluye el día completo.
            var limite = filtro.Hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(b => b.Fecha < limite);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Entidad))
        {
            consulta = consulta.Where(b => b.Entidad == filtro.Entidad);
        }

        return await consulta.OrderByDescending(b => b.Fecha).Take(maximo).ToListAsync();
    }
}
