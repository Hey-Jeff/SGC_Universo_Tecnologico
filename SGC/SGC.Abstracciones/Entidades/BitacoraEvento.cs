namespace SGC.Abstracciones.Entidades;

/// <summary>
/// Registro de auditoría: quién hizo qué, cuándo y sobre qué registro (CA-10.3).
/// Los demás módulos pueden usar la misma tabla mediante IBitacoraService.
/// </summary>
public class BitacoraEvento
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string Entidad { get; set; } = string.Empty;
    public string Registro { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Detalle { get; set; }
}
