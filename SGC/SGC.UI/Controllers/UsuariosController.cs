using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.Abstracciones.Constantes;
using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Interfaces;
using SGC.UI.Extensions;

namespace SGC.UI.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarios;

    public UsuariosController(IUsuarioService usuarios)
    {
        _usuarios = usuarios;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _usuarios.ListarAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        await CargarRolesAsync();
        return View(new UsuarioCrearDto());
    }

    [HttpPost]
    public async Task<IActionResult> Crear(UsuarioCrearDto dto)
    {
        if (!ModelState.IsValid)
        {
            await CargarRolesAsync();
            return View(dto);
        }

        var resultado = await _usuarios.CrearAsync(dto, User.ObtenerId());
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            await CargarRolesAsync();
            return View(dto);
        }

        TempData["Exito"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var usuario = await _usuarios.ObtenerAsync(id);
        if (usuario is null)
        {
            return NotFound();
        }

        await CargarRolesAsync();
        return View(new UsuarioEditarDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            RolIds = usuario.RolIds,
            Activo = usuario.Activo
        });
    }

    [HttpPost]
    public async Task<IActionResult> Editar(UsuarioEditarDto dto)
    {
        if (!ModelState.IsValid)
        {
            await CargarRolesAsync();
            return View(dto);
        }

        var resultado = await _usuarios.EditarAsync(dto, User.ObtenerId());
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            await CargarRolesAsync();
            return View(dto);
        }

        TempData["Exito"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _usuarios.EliminarAsync(id, User.ObtenerId());
        TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarRolesAsync()
    {
        ViewBag.Roles = await _usuarios.ListarRolesAsync();
    }
}
