using System.ComponentModel.DataAnnotations;

namespace SGC.Abstracciones.DTOs;

public class RolDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public List<int> RolIds { get; set; } = new();
    public List<string> RolNombres { get; set; } = new();
    public string RolesDescripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class UsuarioCrearDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme la contraseña.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarPassword { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "Seleccione al menos un rol.")]
    public List<int> RolIds { get; set; } = new();
}

public class UsuarioEditarDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    /// <summary>Opcional: si se deja vacía se conserva la contraseña actual.</summary>
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string? ConfirmarPassword { get; set; }

    [MinLength(1, ErrorMessage = "Seleccione al menos un rol.")]
    public List<int> RolIds { get; set; } = new();

    public bool Activo { get; set; } = true;
}
