using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Phipes.Blog.Abstractions;

/// <summary>
/// Vista mínima del usuario actual que necesita el blog, desacoplada del proveedor de
/// identidad. El host la implementa (o usa la implementación por defecto sobre
/// <see cref="ClaimsPrincipal"/>), de modo que el paquete funcione igual sobre
/// ASP.NET Core Identity clásico o sobre PacificDev.Identity.
/// </summary>
public interface IBlogUserContext
{
    bool IsAuthenticated { get; }

    /// <summary>Id estable del usuario en el proveedor de identidad (NameIdentifier).</summary>
    string? UserId { get; }

    string? DisplayName { get; }

    /// <summary>Puede administrar todo el contenido (rol admin).</summary>
    bool CanManageContent { get; }

    /// <summary>Puede crear/editar contenido propio (rol autor o admin).</summary>
    bool CanAuthor { get; }
}

/// <summary>
/// Implementación por defecto que lee del <see cref="ClaimsPrincipal"/> del
/// <see cref="HttpContext"/>. Reconoce administrador/autor por los nombres de rol
/// configurados en <see cref="PhipesBlogOptions"/>.
/// </summary>
public sealed class HttpContextBlogUserContext(
    IHttpContextAccessor accessor,
    IOptions<PhipesBlogOptions> options) : IBlogUserContext
{
    private readonly PhipesBlogOptions _options = options.Value;

    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string? UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? DisplayName =>
        User?.FindFirstValue("name")
        ?? User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.Identity?.Name;

    public bool CanManageContent =>
        IsAuthenticated && User!.IsInRole(_options.AdminRole);

    public bool CanAuthor =>
        CanManageContent || (IsAuthenticated && User!.IsInRole(_options.AuthorRole));
}
