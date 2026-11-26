using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.Abstracciones.Interfaces;
using SGC.AccesoADatos;
using SGC.LogicaDeNegocio;

var builder = WebApplication.CreateBuilder(args);

//usuario y contraseña por dearrollo

if (builder.Environment.IsDevelopment())
{
    if (string.IsNullOrWhiteSpace(builder.Configuration["AdminInicial:Correo"]))
    {
        builder.Configuration["AdminInicial:Correo"] = "admin@gmail.com";
    }

    if (string.IsNullOrWhiteSpace(builder.Configuration["AdminInicial:Password"]))
    {
        builder.Configuration["AdminInicial:Password"] = "Admin123*";
    }
}

var connectionString = builder.Configuration.GetConnectionString("SgcDb")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'SgcDb' en appsettings.json.");


builder.Services.AddAccesoADatos(connectionString);
builder.Services.AddLogicaDeNegocio();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = "SGC.Auth";
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

await app.Services.InicializarBaseDeDatosAsync();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<IInicializadorService>().InicializarAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();