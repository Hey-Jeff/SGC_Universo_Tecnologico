using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SGC.Abstracciones.Constantes;
using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Entidades;
using SGC.Abstracciones.Interfaces;

namespace SGC.AccesoADatos.Repositorios;

public class SeguimientoRepositorio(SgcDbContext db) : ISeguimientoRepositorio
{
    private IQueryable<SolicitudCredito> Consulta(string[] estados, bool todos) =>
        db.SolicitudesCredito.AsNoTracking().Where(s => todos || estados.Contains(s.Estado));

    public async Task<IReadOnlyList<CreditoFila>> ListarAsync(string[] estados, bool todos) =>
        await Consulta(estados, todos).OrderByDescending(s => s.Id)
            .Select(s => new CreditoFila(s.Id, s.Cliente.Identificacion, s.Cliente.NombreCompleto,
                s.Monto, s.Estado, s.FechaCreacion)).ToListAsync();

    public async Task<CreditoDetalle?> ObtenerAsync(int id, string[] estados, bool todos)
    {
        // Una sola consulta: la autorización por estado y los datos se leen juntos.
        var s = await Consulta(estados, todos).Where(x => x.Id == id).AsSingleQuery()
            .Select(x => new
            {
                x.Id, x.Cliente.Identificacion, Cliente = x.Cliente.NombreCompleto,
                x.Monto, x.Estado, x.FechaCreacion, x.Comentario, x.Version,
                Documentos = x.Documentos.OrderBy(d => d.Id)
                    .Select(d => new DocumentoFila(d.Id, d.Nombre, d.Fecha)).ToList(),
                Historial = x.Movimientos.OrderBy(m => m.Id)
                    .Select(m => new MovimientoFila(m.Id, m.Accion, m.Usuario.Nombre,
                        m.EstadoAnterior, m.EstadoNuevo, m.Comentario, m.Fecha)).ToList()
            }).SingleOrDefaultAsync();

        if (s is null) return null;
        return new CreditoDetalle(new CreditoFila(s.Id, s.Identificacion, s.Cliente, s.Monto,
            s.Estado, s.FechaCreacion), s.Comentario, Convert.ToBase64String(s.Version),
            s.Documentos, s.Historial);
    }

    public async Task<IReadOnlyList<TrackingFila>> ReporteAsync(string[] estados, bool todos, int? gestion) =>
        await db.MovimientosCredito.AsNoTracking()
            .Where(m => (todos || estados.Contains(m.Solicitud.Estado)) &&
                (!gestion.HasValue || m.SolicitudCreditoId == gestion.Value))
            .OrderByDescending(m => m.Id)
            .Select(m => new TrackingFila(m.SolicitudCreditoId, m.Accion, m.Usuario.Nombre,
                m.EstadoAnterior, m.EstadoNuevo, m.Comentario, m.Fecha)).ToListAsync();

    public Task<ArchivoCredito?> DescargarAsync(int id, string[] estados, bool todos) =>
        db.DocumentosCredito.AsNoTracking()
            .Where(d => d.Id == id && (todos || estados.Contains(d.Solicitud.Estado)))
            .Select(d => new ArchivoCredito(d.Id, d.Nombre, d.Contenido)).SingleOrDefaultAsync();

    public async Task<ResultadoOperacion> CambiarAsync(int id, string version, string estadoAnterior,
        string estadoNuevo, string accion, string comentario, int usuario,
        IReadOnlyList<DocumentoCredito> documentos)
    {
        // El índice único filtrado de persona 3 debe proteger también las devoluciones concurrentes.
        var s = await db.SolicitudesCredito.SingleOrDefaultAsync(x => x.Id == id);
        if (s is null || s.Estado != estadoAnterior || Convert.ToBase64String(s.Version) != version)
            return ResultadoOperacion.Fallo("La gestión cambió. Recargue antes de continuar.");

        if (estadoNuevo == EstadosCredito.Devolucion && await db.SolicitudesCredito.AnyAsync(x =>
            x.Id != id && x.ClienteId == s.ClienteId &&
            (x.Estado == EstadosCredito.Registrado || x.Estado == EstadosCredito.Devolucion)))
            return ResultadoOperacion.Fallo("No se puede devolver: el cliente ya tiene otra solicitud pendiente.");

        s.Estado = estadoNuevo;
        s.FechaActualizacion = DateTime.UtcNow;
        // Fuerza la actualización del RowVersion incluso al adjuntar sin cambiar el estado.
        db.Entry(s).Property(x => x.FechaActualizacion).IsModified = true;
        foreach (var d in documentos)
        {
            d.SolicitudCreditoId = id;
            db.DocumentosCredito.Add(d);
        }
        db.MovimientosCredito.Add(new MovimientoCredito
        {
            SolicitudCreditoId = id, UsuarioId = usuario, Accion = accion,
            EstadoAnterior = estadoAnterior, EstadoNuevo = estadoNuevo, Comentario = comentario
        });
        db.Bitacora.Add(new BitacoraEvento
        {
            UsuarioId = usuario, Entidad = "SolicitudCredito", Registro = id.ToString(),
            Accion = accion, Detalle = comentario
        });
        try
        {
            // EF Core ejecuta este SaveChanges en una transacción: estado, documentos e historial.
            await db.SaveChangesAsync();
            return ResultadoOperacion.Ok("Gestión actualizada correctamente.");
        }
        catch (DbUpdateConcurrencyException)
        {
            db.ChangeTracker.Clear();
            return ResultadoOperacion.Fallo("Otro usuario modificó esta gestión. Recargue.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql &&
            (sql.Number == 2601 || sql.Number == 2627 || sql.Number == 1205))
        {
            db.ChangeTracker.Clear();
            return ResultadoOperacion.Fallo(sql.Number == 1205
                ? "Conflicto concurrente. Recargue e intente nuevamente."
                : "No se puede completar: el cliente ya tiene otra solicitud pendiente.");
        }
    }
}
