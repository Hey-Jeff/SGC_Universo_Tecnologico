using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Interfaces;
using SGC.UI.Extensions;

namespace SGC.UI.Controllers;

[Authorize]
public class ClientesController : Controller
{
    private readonly IClienteService _clientes;

    public ClientesController(IClienteService clientes)
    {
        _clientes = clientes;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? filtro = null)
    {
        ViewBag.Filtro = filtro;
        var clientes = await _clientes.ListarAsync(filtro);
        return View(clientes);
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View(new ClienteCrearDto());
    }

    [HttpPost]
    public async Task<IActionResult> Crear(ClienteCrearDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var resultado = await _clientes.CrearAsync(dto, User.ObtenerId());
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(dto);
        }

        TempData["Exito"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var cliente = await _clientes.ObtenerAsync(id);
        if (cliente is null)
        {
            return NotFound();
        }

        return View(new ClienteEditarDto
        {
            Id = cliente.Id,
            Identificacion = cliente.Identificacion,
            NombreCompleto = cliente.NombreCompleto,
            Correo = cliente.Correo,
            Telefono = cliente.Telefono,
            Direccion = cliente.Direccion,
            Activo = cliente.Activo
        });
    }

    [HttpPost]
    public async Task<IActionResult> Editar(ClienteEditarDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var resultado = await _clientes.EditarAsync(dto, User.ObtenerId());
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(dto);
        }

        TempData["Exito"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _clientes.EliminarAsync(id, User.ObtenerId());
        TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }
}