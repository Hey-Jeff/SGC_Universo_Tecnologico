using System.ComponentModel.DataAnnotations;

namespace SGC.Abstracciones.DTOs;

public class SolicitudCreditoCrearDto
{
    [Required(ErrorMessage = "La identificación del cliente es obligatoria.")]
    [StringLength(30, MinimumLength = 6, ErrorMessage = "La identificación debe tener entre 6 y 30 caracteres.")]
    [Display(Name = "Identificación del cliente")]
    public string IdentificacionCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto del crédito es obligatorio.")]
    [Range(typeof(decimal), "1", "10000000", ErrorMessage = "El monto debe ser mayor que ₡0 y no puede superar ₡10.000.000.")]
    [Display(Name = "Monto solicitado (₡)")]
    public decimal Monto { get; set; }

    [StringLength(1000, ErrorMessage = "Los comentarios no pueden superar los 1000 caracteres.")]
    [Display(Name = "Comentarios")]
    public string Comentarios { get; set; } = string.Empty;

    public List<DocumentoSolicitudDto> Documentos { get; set; } = new();
}

public class DocumentoSolicitudDto
{
    public string NombreOriginal { get; set; } = string.Empty;
    public string NombreAlmacenado { get; set; } = string.Empty;
    public string RutaRelativa { get; set; } = string.Empty;
}

public class SolicitudCreditoDto
{
    public int Id { get; set; }
    public string IdentificacionCliente { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Comentarios { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public int CantidadDocumentos { get; set; }
}
