using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Phipes.Blog.Abstractions;
using Phipes.Blog.Data;

namespace Phipes.Blog.DependencyInjection;

/// <summary>
/// Builder fluido para configurar el motor de blog y enchufar los <i>seams</i> del host
/// (resolución de tenant, contexto de usuario, notificador de contacto, base de datos).
/// </summary>
public sealed class PhipesBlogBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    /// <summary>Reemplaza el resolver de tenant por uno del host (ej. lee el claim tenantId).</summary>
    public PhipesBlogBuilder AddTenantResolver<T>() where T : class, IBlogTenantResolver
    {
        Services.AddScoped<IBlogTenantResolver, T>();
        return this;
    }

    /// <summary>Reemplaza el contexto de usuario por uno del host (sobre su ClaimsPrincipal/identidad).</summary>
    public PhipesBlogBuilder AddUserContext<T>() where T : class, IBlogUserContext
    {
        Services.AddScoped<IBlogUserContext, T>();
        return this;
    }

    /// <summary>Enchufa el notificador de mensajes de contacto del host (ej. correo vía Graph).</summary>
    public PhipesBlogBuilder AddContactNotifier<T>() where T : class, IContactNotifier
    {
        Services.RemoveAll<IContactNotifier>();
        Services.AddScoped<IContactNotifier, T>();
        return this;
    }

    /// <summary>
    /// Registra el <see cref="PhipesBlogDbContext"/> con el provider del host
    /// (ej. <c>o =&gt; o.UseSqlServer(connString)</c>).
    /// </summary>
    public PhipesBlogBuilder UseDatabase(Action<DbContextOptionsBuilder> configure)
    {
        Services.AddDbContext<PhipesBlogDbContext>(configure);
        return this;
    }
}
