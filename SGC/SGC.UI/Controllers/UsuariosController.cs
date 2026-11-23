using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            await CargarRolesAsync(dto.RolId);
            return View(dto);
        }

        var resultado = await _usuarios.CrearAsync(dto);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            await CargarRolesAsync(dto.RolId);
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

        await CargarRolesAsync(usuario.RolId);
        return View(new UsuarioEditarDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            RolId = usuario.RolId,
            Activo = usuario.Activo
        });
    }

    [HttpPost]
    public async Task<IActionResult> Editar(UsuarioEditarDto dto)
    {
        if (!ModelState.IsValid)
        {
            await CargarRolesAsync(dto.RolId);
            return View(dto);
        }

        var resultado = await _usuarios.EditarAsync(dto);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            await CargarRolesAsync(dto.RolId);
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

    private async Task CargarRolesAsync(int? seleccionado = null)
    {
        var roles = await _usuarios.ListarRolesAsync();
        ViewBag.Roles = new SelectList(roles, nameof(RolDto.Id), nameof(RolDto.Descripcion), seleccionado);
    }
}
