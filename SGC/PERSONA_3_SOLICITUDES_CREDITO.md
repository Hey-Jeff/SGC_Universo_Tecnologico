# Persona 3 – Creación de solicitudes de crédito

## Funcionalidad incorporada
- Registro de solicitudes asociadas a clientes existentes y activos.
- Monto mayor que cero y máximo de ₡10.000.000 (validación en formulario y servidor).
- Bloqueo cuando el cliente ya tiene una solicitud `Registrado` o `Devolución`.
- Comentarios y carga múltiple de documentos PDF, JPG, JPEG, PNG y DOCX (máximo 10 MB por archivo).
- Estado inicial `Registrado`.
- Listado de solicitudes y registro en la bitácora.
- Acceso al menú para roles Administrador y ServicioCliente.

## Archivos principales
- `SGC/SGC.Abstracciones/Entidades/SolicitudCredito.cs`
- `SGC/SGC.Abstracciones/DTOs/SolicitudCreditoDtos.cs`
- `SGC/SGC.Abstracciones/Interfaces/ISolicitudCredito*.cs`
- `SGC/SGC.AccesoADatos/Repositorios/SolicitudCreditoRepositorio.cs`
- `SGC/SGC.AccesoADatos/Migrations/20261126010000_AgregarSolicitudesCredito.cs`
- `SGC/SGC.LogicaDeNegocio/Servicios/SolicitudCreditoService.cs`
- `SGC/SGC.UI/Controllers/SolicitudesCreditoController.cs`
- `SGC/SGC.UI/Views/SolicitudesCredito/`

## Aplicación
La aplicación ejecuta las migraciones pendientes al iniciar. La nueva migración crea `SolicitudesCredito` y `DocumentosSolicitudCredito`.

## Supuesto que debe confirmar el equipo
El enunciado entregado indica que se debe establecer un estado inicial, pero no especifica su nombre. Este módulo utiliza `Registrado` como estado inicial, ya que ese estado está expresamente mencionado en la regla que bloquea solicitudes duplicadas. Confirmar con el docente/equipo si el flujo define otro estado inicial.

## Almacenamiento de documentos
Los archivos se guardan en `SGC.UI/App_Data/SolicitudesCredito` con nombre generado por el servidor; en SQL Server se guarda el nombre original y la ruta relativa. Los archivos se guardan fuera de `wwwroot` para impedir acceso público directo. Si posteriormente se requiere descargarlos desde la interfaz, debe agregarse una acción de descarga autorizada.
