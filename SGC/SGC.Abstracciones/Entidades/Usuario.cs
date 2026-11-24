namespace SGC.Abstracciones.Entidades;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Borrado lógico: un usuario inactivo no puede iniciar sesión pero conserva su historial.</summary>
    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}
