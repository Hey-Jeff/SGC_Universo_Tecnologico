namespace SGC.Abstracciones.DTOs;

public class ResultadoOperacion
{
    public bool Exito { get; init; }
    public string Mensaje { get; init; } = string.Empty;

    public static ResultadoOperacion Ok(string mensaje = "") => new() { Exito = true, Mensaje = mensaje };
    public static ResultadoOperacion Fallo(string mensaje) => new() { Exito = false, Mensaje = mensaje };
}

public class ResultadoOperacion<T> : ResultadoOperacion
{
    public T? Valor { get; init; }

    public static ResultadoOperacion<T> Ok(T valor, string mensaje = "") =>
        new() { Exito = true, Mensaje = mensaje, Valor = valor };

    public new static ResultadoOperacion<T> Fallo(string mensaje) =>
        new() { Exito = false, Mensaje = mensaje };
}
