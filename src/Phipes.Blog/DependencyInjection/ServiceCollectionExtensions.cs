using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Phipes.Blog.Abstractions;
using Phipes.Blog.Services;

namespace Phipes.Blog.DependencyInjection;

/// <summary>Punto de entrada de DI del motor de blog.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra el motor de blog (servicios, render Markdown, seams por defecto y políticas de
    /// autorización). Devuelve un <see cref="PhipesBlogBuilder"/> para enchufar los seams del host
    /// y la base de datos.
    /// </summary>
    public static PhipesBlogBuilder AddPhipesBlog(
        this IServiceCollection services,
        Action<PhipesBlogOptions>? configure = null)
    {
        services.AddOptions<PhipesBlogOptions>();
        if (configure is not null) services.Configure(configure);

        // Snapshot de roles para las políticas (los nombres de rol no cambian en runtime).
        var snapshot = new PhipesBlogOptions();
        configure?.Invoke(snapshot);

        services.AddHttpContextAccessor();

        // Seams por defecto (el host los puede reemplazar vía el builder).
        services.TryAddScoped<IBlogTenantResolver, DefaultTenantResolver>();
        services.TryAddScoped<IBlogUserContext, HttpContextBlogUserContext>();
        services.TryAddScoped<IContactNotifier, NullContactNotifier>();
        services.TryAddScoped<IImageStorage, WebRootImageStorage>();

        // Servicios.
        services.TryAddSingleton<IMarkdownRenderer, MarkdigMarkdownRenderer>();
        services.AddScoped<IBlogService, EfBlogService>();
        services.AddScoped<IProjectService, EfProjectService>();
        services.AddScoped<IBioService, EfBioService>();
        services.AddScoped<ICommentService, EfCommentService>();
        services.AddScoped<IContactService, EfContactService>();
        services.AddScoped<IResumeService, EfResumeService>();
        services.AddScoped<Seeding.MarkdownContentImporter>();

        // Políticas de autorización basadas en los roles configurados.
        services.AddAuthorizationBuilder()
            .AddPolicy(PhipesBlogPolicies.ManageContent, p => p.RequireRole(snapshot.AdminRole))
            .AddPolicy(PhipesBlogPolicies.Author, p => p.RequireRole(snapshot.AdminRole, snapshot.AuthorRole));

        return new PhipesBlogBuilder(services);
    }
}
