using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.Abstracciones.Constantes;
using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Interfaces;
using SGC.UI.Extensions;

namespace SGC.UI.Controllers;

[Authorize(Roles = Roles.Administrador + "," + Roles.Analista + "," + Roles.Gestor)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class SeguimientoController(ISeguimientoService seguimiento) : Controller
{
    private PermisosCredito Permisos() => new(User.IsInRole(Roles.Administrador),
        User.IsInRole(Roles.Analista), User.IsInRole(Roles.Gestor));

    [HttpGet]
    public async Task<IActionResult> Index() => View(await seguimiento.ListarAsync(Permisos()));

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var detalle = await seguimiento.ObtenerAsync(id, Permisos());
        return detalle is null ? NotFound() : View(detalle);
    }

    [HttpGet]
    public async Task<IActionResult> Reporte(int? gestion)
    {
        if (!ModelState.IsValid || gestion is <= 0) return BadRequest("Número de gestión inválido.");
        ViewBag.Gestion = gestion;
        return View(await seguimiento.ReporteAsync(Permisos(), gestion));
    }

    [HttpGet]
    public async Task<IActionResult> Descargar(int id)
    {
        var documento = await seguimiento.DescargarAsync(id, Permisos());
        if (documento is null) return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(documento.Contenido, "application/octet-stream", documento.Nombre);
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(12 * 1024 * 1024),
        RequestFormLimits(MultipartBodyLengthLimit = 12 * 1024 * 1024)]
    public async Task<IActionResult> Cambiar(MovimientoCrearDto dto, List<IFormFile>? archivos)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Datos de modificación inválidos.";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            var resultado = await seguimiento.CambiarAsync(dto, await CargaDocumentos.LeerAsync(archivos),
                User.ObtenerId(), Permisos());
            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
            if (!resultado.Exito || dto.Accion == "Adjuntar")
            {
                if (await seguimiento.ObtenerAsync(dto.Id, Permisos()) is not null)
                    return RedirectToAction(nameof(Detalle), new { id = dto.Id });
            }
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidDataException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
