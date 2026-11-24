using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.Abstracciones.Constantes;
using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Interfaces;

namespace SGC.UI.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class BitacoraController : Controller
{
    private readonly IBitacoraService _bitacora;

    public BitacoraController(IBitacoraService bitacora)
    {
        _bitacora = bitacora;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] BitacoraFiltroDto filtro)
    {
        ViewBag.Filtro = filtro;
        return View(await _bitacora.ConsultarAsync(filtro));
    }
}
