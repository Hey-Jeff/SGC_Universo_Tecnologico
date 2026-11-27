using Microsoft.EntityFrameworkCore;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;

namespace SGC.AccesoADatos.Repositorios;

public class SolicitudCreditoRepositorio : ISolicitudCreditoRepositorio
{
    private readonly SgcDbContext _db;
    public SolicitudCreditoRepositorio(SgcDbContext db) => _db = db;

    public async Task<IReadOnlyList<SolicitudCredito>> ListarAsync() =>
        await _db.SolicitudesCredito.AsNoTracking()
            .Include(s => s.Documentos)
            .OrderByDescending(s => s.FechaRegistro)
            .ToListAsync();

    public Task<bool> TieneSolicitudActivaAsync(int clienteId) =>
        _db.SolicitudesCredito.AnyAsync(s => s.ClienteId == clienteId &&
            (s.Estado == EstadosSolicitudCredito.Registrado || s.Estado == EstadosSolicitudCredito.Devolucion));

    public async Task AgregarAsync(SolicitudCredito solicitud)
    {
        _db.SolicitudesCredito.Add(solicitud);
        await _db.SaveChangesAsync();
    }
}
