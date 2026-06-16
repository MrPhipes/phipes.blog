using Microsoft.EntityFrameworkCore;
using Phipes.Blog.Data;
using Phipes.Blog.Domain;

namespace Phipes.Blog.Services;

/// <summary>
/// Lectura y administración de las secciones de currículum: skills, testimonios, idiomas
/// y listas simples (música, conferencias). Todas administrables desde el panel.
/// </summary>
public interface IResumeService
{
    Task<IReadOnlyList<Skill>> GetSkillsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Testimonial>> GetTestimonialsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Language>> GetLanguagesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ResumeListItem>> GetListAsync(string listKey, CancellationToken ct = default);

    Task<int> SaveSkillAsync(Skill skill, CancellationToken ct = default);
    Task<int> SaveTestimonialAsync(Testimonial testimonial, CancellationToken ct = default);
    Task<int> SaveLanguageAsync(Language language, CancellationToken ct = default);
    Task<int> SaveListItemAsync(ResumeListItem item, CancellationToken ct = default);

    Task DeleteSkillAsync(int id, CancellationToken ct = default);
    Task DeleteTestimonialAsync(int id, CancellationToken ct = default);
    Task DeleteLanguageAsync(int id, CancellationToken ct = default);
    Task DeleteListItemAsync(int id, CancellationToken ct = default);
}

/// <summary>Implementación EF Core de <see cref="IResumeService"/>.</summary>
public sealed class EfResumeService(PhipesBlogDbContext db) : IResumeService
{
    public async Task<IReadOnlyList<Skill>> GetSkillsAsync(CancellationToken ct = default)
        => await db.Skills.AsNoTracking().OrderBy(s => s.SortOrder).ThenByDescending(s => s.Level).ToListAsync(ct);

    public async Task<IReadOnlyList<Testimonial>> GetTestimonialsAsync(CancellationToken ct = default)
        => await db.Testimonials.AsNoTracking().OrderBy(t => t.SortOrder).ToListAsync(ct);

    public async Task<IReadOnlyList<Language>> GetLanguagesAsync(CancellationToken ct = default)
        => await db.Languages.AsNoTracking().OrderBy(l => l.SortOrder).ToListAsync(ct);

    public async Task<IReadOnlyList<ResumeListItem>> GetListAsync(string listKey, CancellationToken ct = default)
        => await db.ResumeListItems.AsNoTracking()
            .Where(i => i.ListKey == listKey).OrderBy(i => i.SortOrder).ToListAsync(ct);

    public async Task<int> SaveSkillAsync(Skill skill, CancellationToken ct = default)
    {
        if (skill.Id == 0) db.Skills.Add(skill); else db.Skills.Update(skill);
        await db.SaveChangesAsync(ct);
        return skill.Id;
    }

    public async Task<int> SaveTestimonialAsync(Testimonial t, CancellationToken ct = default)
    {
        if (t.Id == 0) db.Testimonials.Add(t); else db.Testimonials.Update(t);
        await db.SaveChangesAsync(ct);
        return t.Id;
    }

    public async Task<int> SaveLanguageAsync(Language l, CancellationToken ct = default)
    {
        if (l.Id == 0) db.Languages.Add(l); else db.Languages.Update(l);
        await db.SaveChangesAsync(ct);
        return l.Id;
    }

    public async Task<int> SaveListItemAsync(ResumeListItem item, CancellationToken ct = default)
    {
        if (item.Id == 0) db.ResumeListItems.Add(item); else db.ResumeListItems.Update(item);
        await db.SaveChangesAsync(ct);
        return item.Id;
    }

    public Task DeleteSkillAsync(int id, CancellationToken ct = default) => DeleteAsync(db.Skills, id, ct);
    public Task DeleteTestimonialAsync(int id, CancellationToken ct = default) => DeleteAsync(db.Testimonials, id, ct);
    public Task DeleteLanguageAsync(int id, CancellationToken ct = default) => DeleteAsync(db.Languages, id, ct);
    public Task DeleteListItemAsync(int id, CancellationToken ct = default) => DeleteAsync(db.ResumeListItems, id, ct);

    private async Task DeleteAsync<T>(DbSet<T> set, int id, CancellationToken ct) where T : class
    {
        var entity = await set.FindAsync([id], ct);
        if (entity is null) return;
        set.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}
