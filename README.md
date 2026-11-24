# SGC · Sistema de Gestión para Universo Tecnológico

Módulo base (HU-10): login, usuarios con varios roles, permisos por rol y bitácora de acciones.
ASP.NET Core MVC · .NET 8 · Entity Framework Core · SQL Server · SweetAlert2.

## Estructura (dependencias en una sola dirección)

```
SGC.UI  →  SGC.LogicaDeNegocio  →  SGC.Abstracciones
   └────→  SGC.AccesoADatos  ───→  SGC.Abstracciones
```

| Proyecto | Contenido |
|---|---|
| `SGC.Abstracciones` | Entidades, DTOs, interfaces y constantes (`Roles`). No depende de nadie. |
| `SGC.AccesoADatos` | `SgcDbContext` y repositorios (EF Core). |
| `SGC.LogicaDeNegocio` | Servicios con todas las reglas de negocio. |
| `SGC.UI` | Controladores delgados, vistas Razor, `Program.cs`. **Sin lógica de negocio.** |

## Cómo ejecutarlo

1. Abra `SGC/SGC.sln` en Visual Studio 2022. El proyecto de inicio es **`SGC.UI`**.
2. Revise la cadena de conexión `SgcDb` en `SGC.UI/appsettings.json` (por defecto LocalDB, sin usuario ni contraseña).
3. Defina las credenciales del administrador inicial. **No se guardan en el repositorio**, se usan *user-secrets*.
   En Visual Studio: clic derecho en `SGC.UI` > **Administrar secretos de usuario** y pegue:

   ```json
   {
     "AdminInicial": {
       "Correo": "admin@universotecnologico.test",
       "Password": "UnaClaveSegura123"
     }
   }
   ```

   O por terminal, desde la carpeta `SGC`:

   ```
   dotnet user-secrets set "AdminInicial:Correo" "admin@universotecnologico.test" --project SGC.UI
   dotnet user-secrets set "AdminInicial:Password" "UnaClaveSegura123" --project SGC.UI
   ```

   Cada integrante lo hace en su propio computador. La contraseña debe tener 8+ caracteres con mayúsculas, minúsculas y números.
4. Ejecute (F5). Se crea la base de datos y, si no hay usuarios, el administrador con esos datos.

> Si ya tenía una base `SGC` creada con una versión anterior, elimínela (SQL Server Object Explorer > clic derecho > Eliminar) para que se cree con el esquema nuevo.

## Base de datos y migraciones

El arranque aplica las migraciones si existen (`MigrateAsync`). Mientras no haya migraciones usa `EnsureCreated`.
**Una sola persona** debe generar la migración inicial y subirla a Git; el resto solo hace `git pull`:

```
dotnet ef migrations add Inicial -p SGC.AccesoADatos -s SGC.UI
```

(Requiere la herramienta: `dotnet tool install --global dotnet-ef`). Cada cambio posterior del modelo se hace con una migración nueva.

## Seguridad y reglas ya implementadas (HU-10)

- **CA-10.1:** solo el rol Administrador puede crear, editar, inactivar usuarios y asignar roles.
- **CA-10.2:** las rutas protegidas se validan en el servidor (`[Authorize(Roles = ...)]`); un usuario sin permiso recibe "Acceso denegado" aunque escriba la URL (`/Usuarios/Crear`).
- **CA-10.3:** crear, editar e inactivar usuarios se registran en la **bitácora** con usuario, acción, fecha y registro afectado (menú Administración > Bitácora de acciones).
- Un usuario puede tener **varios roles** (tabla intermedia `UsuarioRoles`).
- Toda la aplicación requiere sesión por defecto (`FallbackPolicy`); solo `Account/Login` es público.
- Contraseñas con hash (PBKDF2), nunca en texto plano. Antiforgery global en los POST.
- Eliminar es **borrado lógico** (`Activo = false`) para conservar el historial.
- Un administrador no puede eliminarse a sí mismo ni dejar el sistema sin administrador activo.
- Correo único; contraseña con mínimo de seguridad.

## Roles

`Administrador`, `ServicioCliente`, `Analista`, `Gestor` (constantes en `SGC.Abstracciones/Constantes/Roles.cs`).

## Guía para los demás integrantes

1. **Entidades** nuevas en `SGC.Abstracciones/Entidades`, DTOs en `DTOs`, interfaces en `Interfaces`.
2. Agregue su `DbSet` y configuración en `SgcDbContext` y su repositorio en `SGC.AccesoADatos/Repositorios`; regístrelo en `InyeccionDependencias.cs` de esa capa.
3. La lógica va en un servicio de `SGC.LogicaDeNegocio/Servicios`; regístrelo en su `InyeccionDependencias.cs`.
4. Proteja sus pantallas con el rol que corresponda, por ejemplo:
   - `[Authorize]` (cualquier usuario autenticado)
   - `[Authorize(Roles = Roles.Gestor + "," + Roles.Administrador)]`
5. Para saber quién hizo una acción use `User.ObtenerId()` y regístrela en la bitácora desde su servicio:
   ```csharp
   await _bitacora.RegistrarAsync(usuarioId, "OrdenTrabajo", orden.Id.ToString(), "Cerrar", "Se cierra la orden.");
   ```
6. Agregue el enlace de su módulo en `Views/Shared/_Layout.cshtml` y la tarjeta en `Views/Home/Index.cshtml` (hay comentarios marcando el lugar).
7. Nunca suba contraseñas, cadenas de conexión con credenciales ni secretos al repositorio.
