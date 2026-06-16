namespace Phipes.Blog;

/// <summary>
/// Configuración del motor de blog. El host la ajusta vía <c>AddPhipesBlog(o =&gt; ...)</c>.
/// Nada aquí amarra un proveedor de identidad concreto: solo nombres de roles y un tenant
/// por defecto. El cómo se resuelven el usuario y el tenant se inyecta por <i>seams</i>.
/// </summary>
public sealed class PhipesBlogOptions
{
    /// <summary>
    /// Tenant usado cuando el host no resuelve uno (sitio single-tenant como phipes.cl).
    /// </summary>
    public string DefaultTenantId { get; set; } = "default";

    /// <summary>Rol que puede administrar todo el contenido (publicar, moderar, editar ajeno).</summary>
    public string AdminRole { get; set; } = "BlogAdmin";

    /// <summary>Rol que puede crear/editar contenido propio (queda en revisión hasta aprobarse).</summary>
    public string AuthorRole { get; set; } = "BlogAuthor";

    /// <summary>Prefijo de ruta del área pública del blog (ej. "/blog").</summary>
    public string PublicRoutePrefix { get; set; } = "/blog";

    /// <summary>Prefijo de ruta del panel de administración (ej. "/admin/blog").</summary>
    public string AdminRoutePrefix { get; set; } = "/admin/blog";

    /// <summary>Si los comentarios quedan en <c>Pending</c> hasta que un admin los apruebe.</summary>
    public bool ModerateComments { get; set; } = true;

    /// <summary>Si usuarios registrados (rol autor) pueden enviar posts para revisión.</summary>
    public bool AllowExternalAuthors { get; set; } = false;

    /// <summary>Tamaño de página por defecto en los listados públicos.</summary>
    public int PageSize { get; set; } = 10;
}
