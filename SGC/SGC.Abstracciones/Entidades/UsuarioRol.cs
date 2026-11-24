namespace SGC.Abstracciones.Entidades;

/// <summary>Tabla intermedia: un usuario puede tener uno o varios roles (HU-10).</summary>
public class UsuarioRol
{
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;
}
