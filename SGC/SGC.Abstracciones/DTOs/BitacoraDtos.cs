namespace SGC.Abstracciones.DTOs;

public class BitacoraEventoDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string Registro { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string? Detalle { get; set; }
}

public class BitacoraFiltroDto
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public string? Entidad { get; set; }
}
