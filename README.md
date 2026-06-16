# Phipes.Blog

Motor de **blog + proyectos + biografía** autoadministrable para sitios web .NET, empaquetado
como **Razor Class Library**. Pensado para que un sitio personal o corporativo administre su
contenido en runtime (sin recompilar ni migrar por cada post), reutilizable por varios sitios
(**phipes.cl**, **pacificdev.cl**) sobre el mismo esquema.

## Principios de diseño

- **Agnóstico al proveedor de identidad.** El paquete no es dueño de la tabla de usuarios: se
  ancla en un id de usuario `string` y consume `ClaimsPrincipal` + roles. Se monta igual sobre
  **ASP.NET Core Identity** clásico (registro propio + logins externos Google/Facebook) o sobre
  **PacificDev.Identity** (IdentityCore multitenant). El host elige; el paquete pide solo dos
  *seams* chicos.
- **Agnóstico al tema.** La UI usa **Bootstrap 5 básico** (sin templates propietarios), para que
  el paquete sea liberable. Cada sitio le pone su diseño encima vía layout/CSS.
- **Contenido en Markdown.** El cuerpo se edita y guarda como Markdown; se renderiza a HTML en
  lectura (Markdig). Portable y limpio.
- **Multitenant por diseño.** Cada pieza de contenido lleva `TenantId`; filtro global por tenant
  en lecturas y estampado automático en escrituras. Single-tenant = un tenant fijo por defecto.

## Seams (los implementa el host)

| Seam | Responsabilidad | Por defecto |
|---|---|---|
| `IBlogTenantResolver` | Resolver el tenant de la petición | `DefaultTenantResolver` (tenant fijo) |
| `IBlogUserContext` | Exponer usuario/roles actuales | `HttpContextBlogUserContext` (sobre ClaimsPrincipal) |
| `IContactNotifier` | Avisar de un mensaje de contacto (correo, etc.) | `NullContactNotifier` (no-op) |

## Uso

```csharp
builder.Services
    .AddPhipesBlog(o =>
    {
        o.DefaultTenantId = "phipes";
        o.AdminRole = "BlogAdmin";
        o.ModerateComments = true;
    })
    .UseDatabase(db => db.UseSqlServer(connectionString))
    // El host enchufa identidad/tenant/correo cuando los tiene:
    .AddUserContext<MiUserContext>()
    .AddTenantResolver<MiTenantResolver>()
    .AddContactNotifier<GraphContactNotifier>();
```

Las páginas Razor del blog viven en el área `PhipesBlog` (pública + administración) y se sirven
automáticamente al referenciar el paquete.

## Estructura

```
src/Phipes.Blog      la librería (este paquete)
  Domain/            entidades (BlogPost, Project, BioProfile/BioEntry, Comment, ContactMessage, taxonomía)
  Abstractions/      seams de identidad/tenant/notificación
  Data/              PhipesBlogDbContext (EF Core, filtros por tenant)
  Services/          servicios de blog/proyectos/bio/comentarios/contacto + render Markdown
  DependencyInjection/  AddPhipesBlog(...) + PhipesBlogBuilder
  Areas/PhipesBlog/  UI Razor Pages (Bootstrap básico)
samples/WebApp       sitio de ejemplo end-to-end
tests/UnitTests      pruebas
```

## Estado

Alpha (`0.1.0-alpha`). Backend y modelo de dominio funcionales; UI y empaquetado en construcción.

---
© Felipe Hernández (Phipes) · MIT
