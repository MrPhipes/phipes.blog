using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Phipes.Blog.Domain;

namespace Phipes.Blog.Data;

/// <summary>
/// Configuración del modelo del blog, reutilizable por cualquier <c>DbContext</c> (el autónomo
/// <see cref="PhipesBlogDbContext"/> o un contexto de host que herede de <c>IdentityDbContext</c>).
/// Aplica el mapeo de entidades, los filtros globales por tenant y el estampado del tenant.
/// </summary>
public static class PhipesBlogModel
{
    /// <summary>Separador de listas serializadas (unit separator 0x1F, no choca con comas).</summary>
    private const char ListSeparator = (char)0x1F;

    public const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";

    /// <summary>
    /// Convenciones dependientes del provider. SQLite no ordena <see cref="DateTimeOffset"/>:
    /// lo almacenamos como binario ordenable. En SQL Server se deja el tipo nativo.
    /// </summary>
    public static void ConfigureConventions(ModelConfigurationBuilder cb, string? providerName)
    {
        if (providerName == SqliteProviderName)
        {
            cb.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToBinaryConverter>();
            cb.Properties<DateTimeOffset?>().HaveConversion<DateTimeOffsetToBinaryConverter>();
        }
    }

    /// <summary>Mapea las entidades del blog y aplica el filtro por tenant.</summary>
    public static void Configure(ModelBuilder b, string tenantId)
    {
        var csv = new ValueConverter<List<string>, string>(
            v => string.Join(ListSeparator, v),
            v => string.IsNullOrEmpty(v)
                ? new List<string>()
                : v.Split(ListSeparator, StringSplitOptions.None).ToList());

        var stringListComparer = new ValueComparer<List<string>>(
            (a, c) => a!.SequenceEqual(c!),
            v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
            v => v.ToList());

        b.Entity<BlogPost>(e =>
        {
            e.ToTable("BlogPosts");
            e.HasIndex(p => new { p.TenantId, p.Slug }).IsUnique();
            e.HasIndex(p => new { p.TenantId, p.Status, p.PublishedAt });
            e.HasMany(p => p.Categories).WithMany(c => c.Posts);
            e.HasMany(p => p.Tags).WithMany(t => t.Posts);
            e.HasQueryFilter(p => p.TenantId == tenantId);
        });

        b.Entity<Project>(e =>
        {
            e.ToTable("Projects");
            e.HasIndex(p => new { p.TenantId, p.Slug }).IsUnique();
            e.HasMany(p => p.Tags).WithMany(t => t.Projects);
            e.Property(p => p.TechStack).HasConversion(csv).Metadata.SetValueComparer(stringListComparer);
            e.Property(p => p.Highlights)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new())
                .Metadata.SetValueComparer(stringListComparer);
            e.HasQueryFilter(p => p.TenantId == tenantId);
        });

        b.Entity<Category>(e =>
        {
            e.ToTable("BlogCategories");
            e.HasIndex(c => new { c.TenantId, c.Slug }).IsUnique();
            e.HasQueryFilter(c => c.TenantId == tenantId);
        });

        b.Entity<Tag>(e =>
        {
            e.ToTable("BlogTags");
            e.HasIndex(t => new { t.TenantId, t.Slug }).IsUnique();
            e.HasQueryFilter(t => t.TenantId == tenantId);
        });

        b.Entity<Comment>(e =>
        {
            e.ToTable("BlogComments");
            e.HasOne(c => c.BlogPost).WithMany(p => p.Comments)
                .HasForeignKey(c => c.BlogPostId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.ParentComment).WithMany()
                .HasForeignKey(c => c.ParentCommentId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(c => new { c.TenantId, c.BlogPostId, c.Status });
            e.HasQueryFilter(c => c.TenantId == tenantId);
        });

        b.Entity<BioProfile>(e =>
        {
            e.ToTable("BioProfiles");
            e.HasIndex(p => p.TenantId).IsUnique();
            e.HasMany(p => p.Entries).WithOne(x => x.BioProfile!)
                .HasForeignKey(x => x.BioProfileId).OnDelete(DeleteBehavior.Cascade);
            e.Property(p => p.Links)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<BioLink>>(v, (JsonSerializerOptions?)null) ?? new())
                .Metadata.SetValueComparer(new ValueComparer<List<BioLink>>(
                    (a, c) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(c, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
                    v => v.ToList()));
            e.HasQueryFilter(p => p.TenantId == tenantId);
        });

        b.Entity<BioEntry>(e =>
        {
            e.ToTable("BioEntries");
            e.HasIndex(x => new { x.TenantId, x.Kind, x.SortOrder });
            e.HasQueryFilter(x => x.TenantId == tenantId);
        });

        b.Entity<ContactMessage>(e =>
        {
            e.ToTable("ContactMessages");
            e.HasIndex(m => new { m.TenantId, m.Status, m.CreatedAt });
            e.HasQueryFilter(m => m.TenantId == tenantId);
        });

        b.Entity<Skill>(e =>
        {
            e.ToTable("Skills");
            e.HasIndex(s => new { s.TenantId, s.SortOrder });
            e.HasQueryFilter(s => s.TenantId == tenantId);
        });

        b.Entity<Testimonial>(e =>
        {
            e.ToTable("Testimonials");
            e.HasIndex(t => new { t.TenantId, t.SortOrder });
            e.HasQueryFilter(t => t.TenantId == tenantId);
        });

        b.Entity<Language>(e =>
        {
            e.ToTable("Languages");
            e.HasIndex(l => new { l.TenantId, l.SortOrder });
            e.HasQueryFilter(l => l.TenantId == tenantId);
        });

        b.Entity<ResumeListItem>(e =>
        {
            e.ToTable("ResumeListItems");
            e.HasIndex(i => new { i.TenantId, i.ListKey, i.SortOrder });
            e.HasQueryFilter(i => i.TenantId == tenantId);
        });
    }

    /// <summary>Estampa el tenant en las entidades nuevas del blog antes de guardar.</summary>
    public static void StampTenant(ChangeTracker tracker, string tenantId)
    {
        foreach (var entry in tracker.Entries())
        {
            if (entry.State != EntityState.Added) continue;
            if (entry.Metadata.FindProperty("TenantId") is null) continue;

            var current = (string?)entry.Property("TenantId").CurrentValue;
            if (string.IsNullOrEmpty(current))
                entry.Property("TenantId").CurrentValue = tenantId;
        }
    }
}
