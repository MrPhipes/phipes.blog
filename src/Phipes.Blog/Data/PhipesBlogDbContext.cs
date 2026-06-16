using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Phipes.Blog.Abstractions;
using Phipes.Blog.Domain;

namespace Phipes.Blog.Data;

/// <summary>
/// Contexto EF Core del motor de blog. Es independiente de la tabla de usuarios del host
/// (el autor se referencia por id string), así el mismo esquema sirve sobre cualquier
/// proveedor de identidad. El provider (SqlServer/Sqlite) lo aporta el host al registrar
/// el contexto. Aplica filtro global por tenant en lecturas y estampa el tenant en escrituras.
/// </summary>
public class PhipesBlogDbContext : DbContext
{
    /// <summary>Separador interno para listas serializadas como CSV (no choca con comas).</summary>
    private const char ListSeparator = '';

    private readonly string _tenantId;

    public PhipesBlogDbContext(
        DbContextOptions<PhipesBlogDbContext> options,
        IBlogTenantResolver tenantResolver)
        : base(options)
    {
        _tenantId = tenantResolver.ResolveTenantId();
    }

    public DbSet<BlogPost> Posts => Set<BlogPost>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<BioProfile> BioProfiles => Set<BioProfile>();
    public DbSet<BioEntry> BioEntries => Set<BioEntry>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void ConfigureConventions(ModelConfigurationBuilder cb)
    {
        base.ConfigureConventions(cb);

        // SQLite no ordena DateTimeOffset en SQL. Para que el mismo esquema corra también
        // sobre SQLite (samples/tests) lo almacenamos como binario ordenable. En SQL Server
        // se deja el tipo nativo. Detección por nombre para no acoplar el paquete al provider.
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            cb.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToBinaryConverter>();
            cb.Properties<DateTimeOffset?>().HaveConversion<DateTimeOffsetToBinaryConverter>();
        }
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

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
            e.HasQueryFilter(p => p.TenantId == _tenantId);
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
            e.HasQueryFilter(p => p.TenantId == _tenantId);
        });

        b.Entity<Category>(e =>
        {
            e.ToTable("BlogCategories");
            e.HasIndex(c => new { c.TenantId, c.Slug }).IsUnique();
            e.HasQueryFilter(c => c.TenantId == _tenantId);
        });

        b.Entity<Tag>(e =>
        {
            e.ToTable("BlogTags");
            e.HasIndex(t => new { t.TenantId, t.Slug }).IsUnique();
            e.HasQueryFilter(t => t.TenantId == _tenantId);
        });

        b.Entity<Comment>(e =>
        {
            e.ToTable("BlogComments");
            e.HasOne(c => c.BlogPost).WithMany(p => p.Comments)
                .HasForeignKey(c => c.BlogPostId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.ParentComment).WithMany()
                .HasForeignKey(c => c.ParentCommentId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(c => new { c.TenantId, c.BlogPostId, c.Status });
            e.HasQueryFilter(c => c.TenantId == _tenantId);
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
            e.HasQueryFilter(p => p.TenantId == _tenantId);
        });

        b.Entity<BioEntry>(e =>
        {
            e.ToTable("BioEntries");
            e.HasIndex(x => new { x.TenantId, x.Kind, x.SortOrder });
            e.HasQueryFilter(x => x.TenantId == _tenantId);
        });

        b.Entity<ContactMessage>(e =>
        {
            e.ToTable("ContactMessages");
            e.HasIndex(m => new { m.TenantId, m.Status, m.CreatedAt });
            e.HasQueryFilter(m => m.TenantId == _tenantId);
        });
    }

    /// <summary>Estampa el tenant resuelto en las entidades nuevas antes de guardar.</summary>
    public override int SaveChanges()
    {
        StampTenant();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampTenant();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampTenant()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added) continue;
            if (entry.Metadata.FindProperty("TenantId") is null) continue;

            var current = (string?)entry.Property("TenantId").CurrentValue;
            if (string.IsNullOrEmpty(current))
                entry.Property("TenantId").CurrentValue = _tenantId;
        }
    }
}
