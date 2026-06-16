using Microsoft.Extensions.Options;

namespace Phipes.Blog.Abstractions;

/// <summary>
/// Resuelve el tenant activo para la petición en curso. Es el <i>seam</i> de multitenancy:
/// en un sitio single-tenant (phipes.cl) devuelve un valor fijo; en un host multitenant
/// (estilo PacificDev.Identity/Lighthouse) lo lee del claim <c>tenantId</c> o del host.
/// </summary>
public interface IBlogTenantResolver
{
    string ResolveTenantId();
}

/// <summary>Resolver por defecto: siempre devuelve <see cref="PhipesBlogOptions.DefaultTenantId"/>.</summary>
public sealed class DefaultTenantResolver(IOptions<PhipesBlogOptions> options) : IBlogTenantResolver
{
    private readonly string _tenantId = options.Value.DefaultTenantId;

    public string ResolveTenantId() => _tenantId;
}
