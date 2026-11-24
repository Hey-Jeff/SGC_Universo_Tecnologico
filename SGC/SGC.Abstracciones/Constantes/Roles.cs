namespace SGC.Abstracciones.Constantes;

/// <summary>
/// Nombres de los roles del sistema. Se usan en [Authorize(Roles = Roles.X)] y en los claims.
/// </summary>
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string ServicioCliente = "ServicioCliente";
    public const string Analista = "Analista";
    public const string Gestor = "Gestor";

    public static readonly string[] Todos = { Administrador, ServicioCliente, Analista, Gestor };
}
