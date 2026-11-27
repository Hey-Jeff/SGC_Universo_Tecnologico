namespace SGC.Abstracciones.Entidades;

public static class EstadosSolicitudCredito
{
    public const string Registrado = "Registrado";
    public const string Devolucion = "Devolución";
}

public class SolicitudCredito
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public decimal Monto { get; set; }
    public string Comentarios { get; set; } = string.Empty;
    public string Estado { get; set; } = EstadosSolicitudCredito.Registrado;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public int UsuarioRegistroId { get; set; }
    public ICollection<DocumentoSolicitudCredito> Documentos { get; set; } = new List<DocumentoSolicitudCredito>();
}

public class DocumentoSolicitudCredito
{
    public int Id { get; set; }
    public int SolicitudCreditoId { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string NombreAlmacenado { get; set; } = string.Empty;
    public string RutaRelativa { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;
    public SolicitudCredito? Solicitud { get; set; }
}
