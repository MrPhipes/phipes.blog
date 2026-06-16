using Microsoft.EntityFrameworkCore;
using Phipes.Blog.Data;
using Phipes.Blog.Domain;

namespace WebApp;

/// <summary>Carga contenido de ejemplo para que el sample muestre el motor funcionando.</summary>
public static class SampleData
{
    public static async Task SeedAsync(PhipesBlogDbContext db)
    {
        if (await db.Posts.IgnoreQueryFilters().AnyAsync()) return;

        var now = DateTimeOffset.UtcNow;

        var dotnet = new Category { Name = ".NET", Slug = "dotnet" };
        var homelab = new Category { Name = "Homelab", Slug = "homelab" };

        db.Posts.AddRange(
            new BlogPost
            {
                Title = "Hola, Phipes.Blog",
                Slug = "hola-phipes-blog",
                Summary = "La primera entrada servida por el motor de blog reutilizable.",
                BodyMarkdown = """
                    # Hola, Phipes.Blog

                    Esta entrada se renderiza desde **Markdown** guardado en la base de datos.

                    - Autoadministrable
                    - Agnóstico al tema (Bootstrap básico)
                    - Reutilizable por varios sitios

                    ```csharp
                    builder.Services.AddPhipesBlog().UseDatabase(db => db.UseSqlite("..."));
                    ```
                    """,
                Status = ContentStatus.Published,
                PublishedAt = now.AddDays(-2),
                IsFeatured = true,
                ReadingMinutes = 2,
                AuthorDisplayName = "Administrador Demo",
                Categories = { dotnet },
                Tags = { new Tag { Name = "markdown", Slug = "markdown" } },
            },
            new BlogPost
            {
                Title = "Montando un homelab",
                Slug = "montando-un-homelab",
                Summary = "Notas sobre el armado de un laboratorio casero.",
                BodyMarkdown = "## Homelab\n\nContenido de ejemplo sobre el homelab.",
                Status = ContentStatus.Published,
                PublishedAt = now.AddDays(-1),
                ReadingMinutes = 1,
                AuthorDisplayName = "Administrador Demo",
                Categories = { homelab },
            });

        db.Projects.Add(new Project
        {
            Title = "Plataforma omnicanal integrada a SAP",
            Slug = "omnicanal-sap",
            Summary = "Integración de ventas omnicanal con el ERP.",
            Outcome = "−40% en tiempo de procesamiento de pedidos.",
            BodyMarkdown = "## Contexto\n\nCaso comercial de ejemplo.\n\n## Solución\n\nIntegración punta a punta.",
            ClientName = "Retail (confidencial)",
            Role = "Arquitecto e implementador",
            Status = ContentStatus.Published,
            PublishedAt = now.AddDays(-3),
            IsFeatured = true,
            AuthorDisplayName = "Administrador Demo",
            TechStack = { ".NET", "SAP Service Layer", "SQL Server" },
            Highlights = { "Integración en tiempo real", "Sin downtime en el corte" },
        });

        db.BioProfiles.Add(new BioProfile
        {
            DisplayName = "Felipe Hernández",
            Headline = "Ingeniero de software · Integraciones .NET / SAP",
            SummaryMarkdown = "Desarrollador de soluciones a medida. Este es un texto de ejemplo.",
            UpdatedAt = now,
            Links = { new BioLink { Label = "GitHub", Url = "https://github.com/MrPhipes" } },
            Entries =
            {
                new BioEntry
                {
                    Kind = BioEntryKind.Experience, Title = "Desarrollador de soluciones",
                    Organization = "Phipes", StartDate = new DateOnly(2020, 1, 1), IsCurrent = true,
                    DescriptionMarkdown = "Integraciones y desarrollo a medida.",
                },
                new BioEntry
                {
                    Kind = BioEntryKind.Education, Title = "Ingeniería en Informática",
                    Organization = "Instituto (ejemplo)", StartDate = new DateOnly(2012, 3, 1),
                    EndDate = new DateOnly(2016, 12, 1),
                },
            },
        });

        await db.SaveChangesAsync();
    }
}
