# SGC · Seguimiento de gestiones de crédito

Base del proyecto (Persona 1): login, administración de usuarios, roles y estructura MVC en capas.
ASP.NET Core MVC · .NET 8 · Entity Framework Core · SQL Server.

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

1. Abra `SGC.sln` en Visual Studio 2022 (o use `dotnet run --project SGC.UI`).
2. Revise la cadena de conexión `SgcDb` en `SGC.UI/appsettings.json` (por defecto LocalDB).
3. Al arrancar se crea la base de datos y el usuario inicial:
   - Correo: `admin@sgc.com`
   - Contraseña: `Admin123*` (cámbiela desde **Usuarios > Editar**)

## Seguridad y reglas ya implementadas

- Inicio de sesión con cookies; contraseñas con hash (PBKDF2), nunca en texto plano.
- **Toda la aplicación requiere sesión** por defecto (`FallbackPolicy`). Solo `Account/Login` es público.
- Roles: `Administrador`, `ServicioCliente`, `Analista`, `Gestor` (constantes en `Roles`).
- Usuarios solo para el administrador. Eliminar es **borrado lógico** (`Activo = false`) para no romper el historial.
- Un administrador no puede eliminarse a sí mismo ni dejar el sistema sin administrador activo.
- Contraseña: mínimo 8 caracteres con mayúsculas, minúsculas y números. Correo único.
- Antiforgery global en los POST; confirmaciones y avisos con SweetAlert2.

## Guía para los demás integrantes

1. **Entidades** nuevas en `SGC.Abstracciones/Entidades`, DTOs en `DTOs`, interfaces en `Interfaces`.
2. Agregue su `DbSet` y configuración en `SgcDbContext` y su repositorio en `SGC.AccesoADatos/Repositorios`; regístrelo en `InyeccionDependencias.cs` de esa capa.
3. La lógica va en un servicio de `SGC.LogicaDeNegocio/Servicios`; regístrelo en su `InyeccionDependencias.cs`.
4. Proteja sus pantallas con el rol correspondiente:
   - Clientes: `[Authorize]` (todos los roles)
   - Solicitudes: `[Authorize(Roles = Roles.ServicioCliente + "," + Roles.Administrador)]`
   - Seguimiento: `[Authorize(Roles = Roles.Analista + "," + Roles.Gestor + "," + Roles.Administrador)]`
5. Para saber quién hizo una acción (tracking), use `User.ObtenerId()`.
6. Agregue el enlace de su módulo en `Views/Shared/_Layout.cshtml` y la tarjeta en `Views/Home/Index.cshtml` (hay comentarios marcando el lugar).

### Base de datos en equipo

Al inicio el esquema se crea con `EnsureCreated`, que **no modifica** una base que ya existe. En cuanto alguien agregue tablas, usen migraciones (un solo integrante genera la migración y la sube a Git):

```
dotnet ef migrations add Inicial -p SGC.AccesoADatos -s SGC.UI
```

Desde que existan migraciones, el arranque las aplica automáticamente (`MigrateAsync`).
