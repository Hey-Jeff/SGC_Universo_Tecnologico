namespace SGC.Abstracciones.Constantes;


/// Nombres de los roles del sistema

public static class Roles
{
    public const string Administrador = "Administrador";
    public const string ServicioCliente = "ServicioCliente";
    public const string Analista = "Analista";
    public const string Gestor = "Gestor";

    public static readonly string[] Todos = { Administrador, ServicioCliente, Analista, Gestor };
}
