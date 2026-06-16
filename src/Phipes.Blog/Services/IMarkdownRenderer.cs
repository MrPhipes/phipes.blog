using Markdig;

namespace Phipes.Blog.Services;

/// <summary>Convierte Markdown (la fuente del contenido) a HTML para mostrar.</summary>
public interface IMarkdownRenderer
{
    string ToHtml(string? markdown);

    /// <summary>Extrae un resumen en texto plano (para meta/og) de un Markdown.</summary>
    string ToPlainText(string? markdown, int maxLength = 280);
}

/// <summary>Implementación sobre Markdig con extensiones avanzadas (tablas, autolinks, etc.).</summary>
public sealed class MarkdigMarkdownRenderer : IMarkdownRenderer
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseSoftlineBreakAsHardlineBreak()
        .Build();

    public string ToHtml(string? markdown)
        => string.IsNullOrWhiteSpace(markdown) ? string.Empty : Markdown.ToHtml(markdown, _pipeline);

    public string ToPlainText(string? markdown, int maxLength = 280)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return string.Empty;
        var text = Markdown.ToPlainText(markdown, _pipeline).Replace('\n', ' ').Replace('\r', ' ').Trim();
        while (text.Contains("  ")) text = text.Replace("  ", " ");
        return text.Length <= maxLength ? text : text[..maxLength].TrimEnd() + "…";
    }
}
