using System.ComponentModel.DataAnnotations;

namespace SGC.Abstracciones.DTOs;

public record PermisosCredito(bool Administrador, bool Analista, bool Gestor);

public class MovimientoCrearDto
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }
    [Required, StringLength(50)]
    public string Version { get; set; } = string.Empty;
    [Required, StringLength(20)]
    public string Accion { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Comentario { get; set; }
}

public record TrackingFila(int Gestion, string Accion, string Usuario, string? EstadoAnterior,
    string EstadoNuevo, string Comentario, DateTime Fecha);
public record ArchivoCredito(int Id, string Nombre, byte[] Contenido);
public record SeguimientoVista(CreditoDetalle Detalle, bool PuedeAnalizar, bool PuedeDecidir);
