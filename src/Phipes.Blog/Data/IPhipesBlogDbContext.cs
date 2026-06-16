using Microsoft.EntityFrameworkCore;
using Phipes.Blog.Domain;

namespace Phipes.Blog.Data;

/// <summary>
/// Superficie del contexto de datos que necesita el motor de blog. La implementa tanto el
/// contexto autónomo <see cref="PhipesBlogDbContext"/> como —en un host con identidad— un
/// único <c>DbContext</c> que herede de <c>IdentityDbContext</c> e incluya el modelo del blog
/// (patrón "una sola base", al estilo de DimacoIntranet). Así Identity y el blog conviven en
/// una sola implementación y una sola base de datos.
/// </summary>
public interface IPhipesBlogDbContext
{
    DbSet<BlogPost> Posts { get; }
    DbSet<Project> Projects { get; }
    DbSet<Category> Categories { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Comment> Comments { get; }
    DbSet<BioProfile> BioProfiles { get; }
    DbSet<BioEntry> BioEntries { get; }
    DbSet<ContactMessage> ContactMessages { get; }
    DbSet<Skill> Skills { get; }
    DbSet<Testimonial> Testimonials { get; }
    DbSet<Language> Languages { get; }
    DbSet<ResumeListItem> ResumeListItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
