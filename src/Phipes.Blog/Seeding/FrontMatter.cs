using Phipes.Blog.Domain;

namespace Phipes.Blog.Seeding;

/// <summary>
/// Parser mínimo de front-matter YAML para los archivos de contenido migrado. Soporta el
/// subconjunto que usamos: escalares (<c>key: value</c> / <c>key: "value"</c> / <c>key: null</c>),
/// listas de strings (líneas <c>- item</c>) y la lista de enlaces de la bio (mapas label/url).
/// No pretende ser un parser YAML completo.
/// </summary>
internal sealed class FrontMatter
{
    public Dictionary<string, string?> Scalars { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, List<string>> Lists { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<BioLink> Links { get; } = new();

    public string? Str(string key) => Scalars.TryGetValue(key, out var v) ? v : null;

    public bool Bool(string key) => bool.TryParse(Str(key), out var b) && b;

    public List<string> List(string key) => Lists.TryGetValue(key, out var v) ? v : new();

    public DateTimeOffset? Date(string key)
        => DateTimeOffset.TryParse(Str(key), null, System.Globalization.DateTimeStyles.RoundtripKind, out var d)
            ? d : null;

    /// <summary>Separa un documento en (front-matter, cuerpo). El front-matter va entre dos líneas "---".</summary>
    public static (FrontMatter fm, string body) Parse(string content)
    {
        var fm = new FrontMatter();
        content = content.Replace("\r\n", "\n");

        if (!content.StartsWith("---\n"))
            return (fm, content.Trim());

        var end = content.IndexOf("\n---", 4, StringComparison.Ordinal);
        if (end < 0) return (fm, content.Trim());

        var header = content[4..end];
        var body = content[(end + 4)..].TrimStart('\n').Trim();

        string? currentList = null;
        BioLink? currentLink = null;

        foreach (var raw in header.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var indent = raw.Length - raw.TrimStart().Length;
            var line = raw.Trim();

            if (indent == 0)
            {
                currentList = null;
                currentLink = null;
                var idx = line.IndexOf(':');
                if (idx < 0) continue;
                var key = line[..idx].Trim();
                var val = line[(idx + 1)..].Trim();
                if (val.Length > 0)
                {
                    fm.Scalars[key] = Unquote(val);
                }
                else
                {
                    currentList = key;
                    fm.Lists[key] = new List<string>();
                }
            }
            else if (currentList is not null)
            {
                if (line.StartsWith("- "))
                {
                    var item = line[2..].Trim();
                    if (currentList.Equals("links", StringComparison.OrdinalIgnoreCase) && item.Contains(':'))
                    {
                        currentLink = new BioLink();
                        ApplyLinkKv(currentLink, item);
                        fm.Links.Add(currentLink);
                    }
                    else
                    {
                        fm.Lists[currentList].Add(Unquote(item) ?? string.Empty);
                    }
                }
                else if (currentLink is not null)
                {
                    ApplyLinkKv(currentLink, line);
                }
            }
        }

        return (fm, body);
    }

    private static void ApplyLinkKv(BioLink link, string kv)
    {
        var idx = kv.IndexOf(':');
        if (idx < 0) return;
        var key = kv[..idx].Trim();
        var val = Unquote(kv[(idx + 1)..].Trim()) ?? string.Empty;
        if (key.Equals("label", StringComparison.OrdinalIgnoreCase)) link.Label = val;
        else if (key.Equals("url", StringComparison.OrdinalIgnoreCase)) link.Url = val;
    }

    private static string? Unquote(string v)
    {
        if (v.Equals("null", StringComparison.OrdinalIgnoreCase)) return null;
        if (v.Length >= 2 && ((v[0] == '"' && v[^1] == '"') || (v[0] == '\'' && v[^1] == '\'')))
            return v[1..^1];
        return v;
    }
}
