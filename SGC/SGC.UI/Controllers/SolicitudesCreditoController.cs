using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGC.Abstracciones.Constantes;
using SGC.Abstracciones.DTOs;
using SGC.Abstracciones.Interfaces;
using SGC.UI.Extensions;

namespace SGC.UI.Controllers;

[Authorize(Roles = Roles.Administrador + "," + Roles.ServicioCliente)]
public class SolicitudesCreditoController : Controller
{
    private static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
        { ".pdf", ".jpg", ".jpeg", ".png", ".docx" };
    private const long TamanoMaximoArchivo = 10 * 1024 * 1024;
    private readonly ISolicitudCreditoService _solicitudes;
    private readonly IWebHostEnvironment _environment;

    public SolicitudesCreditoController(ISolicitudCreditoService solicitudes, IWebHostEnvironment environment)
    {
        _solicitudes = solicitudes;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index() => View(await _solicitudes.ListarAsync());

    [HttpGet]
    public IActionResult Crear() => View(new SolicitudCreditoCrearDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(SolicitudCreditoCrearDto dto, List<IFormFile>? archivos)
    {
        var archivosValidos = new List<IFormFile>();
        if (archivos is not null)
        {
            foreach (var archivo in archivos.Where(a => a.Length > 0))
            {
                var extension = Path.GetExtension(archivo.FileName);
                if (!ExtensionesPermitidas.Contains(extension))
                    ModelState.AddModelError(nameof(archivos), $"El archivo {Path.GetFileName(archivo.FileName)} no tiene un formato permitido. Use PDF, JPG, PNG o DOCX.");
                else if (archivo.Length > TamanoMaximoArchivo)
                    ModelState.AddModelError(nameof(archivos), $"El archivo {Path.GetFileName(archivo.FileName)} supera el límite de 10 MB.");
                else archivosValidos.Add(archivo);
            }
        }

        if (!ModelState.IsValid) return View(dto);

        // Los documentos se guardan fuera de wwwroot para evitar acceso público directo.
        var carpetaFisica = Path.Combine(_environment.ContentRootPath, "App_Data", "SolicitudesCredito");
        Directory.CreateDirectory(carpetaFisica);
        var guardados = new List<string>();
        try
        {
            foreach (var archivo in archivosValidos)
            {
                var nombreGuardado = $"{Guid.NewGuid():N}{Path.GetExtension(archivo.FileName).ToLowerInvariant()}";
                var rutaFisica = Path.Combine(carpetaFisica, nombreGuardado);
                await using (var stream = new FileStream(rutaFisica, FileMode.CreateNew))
                    await archivo.CopyToAsync(stream);
                guardados.Add(rutaFisica);
                dto.Documentos.Add(new DocumentoSolicitudDto
                {
                    NombreOriginal = Path.GetFileName(archivo.FileName),
                    NombreAlmacenado = nombreGuardado,
                    RutaRelativa = Path.Combine("App_Data", "SolicitudesCredito", nombreGuardado)
                });
            }

            var resultado = await _solicitudes.CrearAsync(dto, User.ObtenerId());
            if (!resultado.Exito)
            {
                foreach (var ruta in guardados) if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(dto);
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            foreach (var ruta in guardados) if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
            ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar la solicitud o sus documentos. Intente nuevamente.");
            return View(dto);
        }
    }
}
