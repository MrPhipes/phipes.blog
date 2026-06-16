using Microsoft.EntityFrameworkCore;
using Phipes.Blog.Abstractions;
using Phipes.Blog.Domain;

namespace Phipes.Blog.Data;

/// <summary>
/// Contexto EF Core autónomo del motor de blog, para hosts que NO unifican con Identity
/// (samples, pruebas, o sitios sin login). Un host con identidad usa en cambio un único
/// <c>DbContext</c> que herede de <c>IdentityDbContext</c> e implemente
/// <see cref="IPhipesBlogDbContext"/> aplicando <see cref="PhipesBlogModel"/>.
/// </summary>
public class PhipesBlogDbContext : DbContext, IPhipesBlogDbContext
{
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
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<ResumeListItem> ResumeListItems => Set<ResumeListItem>();

    protected override void ConfigureConventions(ModelConfigurationBuilder cb)
    {
        base.ConfigureConventions(cb);
        PhipesBlogModel.ConfigureConventions(cb, Database.ProviderName);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        PhipesBlogModel.Configure(b, _tenantId);
    }

    public override int SaveChanges()
    {
        PhipesBlogModel.StampTenant(ChangeTracker, _tenantId);
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PhipesBlogModel.StampTenant(ChangeTracker, _tenantId);
        return base.SaveChangesAsync(cancellationToken);
    }
}
