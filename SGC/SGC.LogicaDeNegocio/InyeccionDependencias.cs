using Microsoft.Extensions.DependencyInjection;
using SGC.Abstracciones.Interfaces;
using SGC.LogicaDeNegocio.Servicios;

namespace SGC.LogicaDeNegocio;

public static class InyeccionDependencias
{
    public static IServiceCollection AddLogicaDeNegocio(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IInicializadorService, InicializadorService>();

        return services;
    }
}
