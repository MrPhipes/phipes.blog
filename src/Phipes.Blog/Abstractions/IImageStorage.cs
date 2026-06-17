using Microsoft.AspNetCore.Hosting;

namespace Phipes.Blog.Abstractions;

/// <summary>
/// Almacenamiento de imágenes subidas desde el panel. Devuelve la URL pública que queda
/// guardada en el contenido (ej. <c>CoverImageUrl</c>). El host puede reemplazar la
/// implementación por una que use blob storage, CDN, etc.
/// </summary>
public interface IImageStorage
{
    /// <summary>Guarda el archivo y devuelve la URL pública para referenciarlo.</summary>
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct = default);
}

/// <summary>
/// Implementación por defecto: guarda en <c>wwwroot/uploads/&lt;tenant&gt;/</c> y devuelve
/// <c>/uploads/&lt;tenant&gt;/&lt;archivo&gt;</c>. Sirve para la mayoría de los sitios.
/// </summary>
public sealed class WebRootImageStorage(
    IWebHostEnvironment env,
    IBlogTenantResolver tenantResolver) : IImageStorage
{
    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct = default)
    {
        var tenant = Services.Slug.From(tenantResolver.ResolveTenantId());
        if (string.IsNullOrEmpty(tenant)) tenant = "default";

        var ext = Path.GetExtension(fileName);
        var baseName = Services.Slug.From(Path.GetFileNameWithoutExtension(fileName));
        if (string.IsNullOrEmpty(baseName)) baseName = "img";
        var unique = $"{baseName}-{Guid.NewGuid():N}{ext}".ToLowerInvariant();

        var webRoot = env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var dir = Path.Combine(webRoot, "uploads", tenant);
        Directory.CreateDirectory(dir);

        var fullPath = Path.Combine(dir, unique);
        await using (var fs = File.Create(fullPath))
        {
            await content.CopyToAsync(fs, ct);
        }

        return $"/uploads/{tenant}/{unique}";
    }
}
