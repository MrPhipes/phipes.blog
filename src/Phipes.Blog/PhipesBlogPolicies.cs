namespace Phipes.Blog;

/// <summary>Nombres canónicos de las políticas de autorización del blog.</summary>
public static class PhipesBlogPolicies
{
    /// <summary>Administrar todo el contenido: publicar, moderar, editar de otros.</summary>
    public const string ManageContent = "PhipesBlog.ManageContent";

    /// <summary>Crear/editar contenido propio.</summary>
    public const string Author = "PhipesBlog.Author";
}
