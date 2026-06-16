namespace Phipes.Blog.Services;

/// <summary>Lectura pública y administración de los proyectos (casos comerciales/portafolio).</summary>
public interface IProjectService
{
    Task<PagedResult<ProjectListItem>> GetPublishedAsync(int page = 1, int? pageSize = null, string? tagSlug = null, CancellationToken ct = default);
    Task<ProjectDetail?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectListItem>> GetFeaturedAsync(int count = 3, CancellationToken ct = default);

    Task<PagedResult<ProjectListItem>> ListAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<ProjectEditModel?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<int> SaveAsync(ProjectEditModel model, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
