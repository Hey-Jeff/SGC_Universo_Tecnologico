using System.ComponentModel.DataAnnotations;

namespace SGC.Abstracciones.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class ClienteCrearDto
{
    [Required(ErrorMessage = "La identificación es obligatoria.")]
    [StringLength(30, MinimumLength = 6, ErrorMessage = "La identificación debe tener entre 6 y 30 caracteres.")]
    public string Identificacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(25, MinimumLength = 7, ErrorMessage = "El teléfono debe tener entre 7 y 25 caracteres.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(250, ErrorMessage = "La dirección no puede superar los 250 caracteres.")]
    public string Direccion { get; set; } = string.Empty;
}

public class ClienteEditarDto : ClienteCrearDto
{
    public int Id { get; set; }
    public bool Activo { get; set; } = true;
}