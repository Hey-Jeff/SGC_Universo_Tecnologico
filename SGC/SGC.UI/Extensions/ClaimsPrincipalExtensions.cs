using System.Security.Claims;

namespace SGC.UI.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Id del usuario autenticado (0 si no hay sesión). Úselo para registrar quién hizo cada acción.</summary>
    public static int ObtenerId(this ClaimsPrincipal user)
    {
        var valor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var id) ? id : 0;
    }

    public static string ObtenerNombre(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
}
