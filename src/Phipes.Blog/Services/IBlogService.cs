using Phipes.Blog.Domain;

namespace Phipes.Blog.Services;

/// <summary>Operaciones de lectura pública y de administración sobre las entradas de blog.</summary>
public interface IBlogService
{
    // ---- Lectura pública (solo contenido publicado) ----
    Task<PagedResult<PostListItem>> GetPublishedAsync(
        int page = 1, int? pageSize = null, string? categorySlug = null, string? tagSlug = null,
        CancellationToken ct = default);

    Task<PostDetail?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default);

    Task<IReadOnlyList<PostListItem>> GetFeaturedAsync(int count = 3, CancellationToken ct = default);

    // ---- Administración ----
    Task<PagedResult<PostListItem>> ListAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<PostEditModel?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<int> SaveAsync(PostEditModel model, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
