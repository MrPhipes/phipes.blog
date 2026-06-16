using Microsoft.EntityFrameworkCore;
using Phipes.Blog.Data;
using Phipes.Blog.Domain;

namespace Phipes.Blog.Services;

/// <summary>Lectura y edición del perfil biográfico y su línea de tiempo.</summary>
public interface IBioService
{
    Task<BioView?> GetAsync(CancellationToken ct = default);
    Task<BioProfile> GetOrCreateProfileAsync(CancellationToken ct = default);
    Task SaveProfileAsync(BioProfile profile, CancellationToken ct = default);
    Task<int> SaveEntryAsync(BioEntry entry, CancellationToken ct = default);
    Task DeleteEntryAsync(int entryId, CancellationToken ct = default);
}

/// <summary>Implementación EF Core de <see cref="IBioService"/>.</summary>
public sealed class EfBioService(IPhipesBlogDbContext db, IMarkdownRenderer markdown) : IBioService
{
    public async Task<BioView?> GetAsync(CancellationToken ct = default)
    {
        var profile = await db.BioProfiles.AsNoTracking()
            .Include(p => p.Entries)
            .FirstOrDefaultAsync(ct);
        if (profile is null) return null;

        var entries = profile.Entries
            .OrderByDescending(e => e.IsCurrent)
            .ThenByDescending(e => e.StartDate)
            .ThenBy(e => e.SortOrder)
            .Select(e => new BioEntryView(
                e.Kind, e.Title, e.Organization, e.Location, e.StartDate, e.EndDate, e.IsCurrent,
                string.IsNullOrWhiteSpace(e.DescriptionMarkdown) ? null : markdown.ToHtml(e.DescriptionMarkdown)))
            .ToList();

        return new BioView(
            profile.DisplayName, profile.Headline, profile.AvatarUrl,
            markdown.ToHtml(profile.SummaryMarkdown), profile.ContactEmail,
            profile.Location, profile.WebsiteUrl, profile.GithubUsername,
            profile.Latitude, profile.Longitude,
            profile.Links, entries);
    }

    public async Task<BioProfile> GetOrCreateProfileAsync(CancellationToken ct = default)
    {
        var profile = await db.BioProfiles.Include(p => p.Entries).FirstOrDefaultAsync(ct);
        if (profile is null)
        {
            profile = new BioProfile { DisplayName = "", UpdatedAt = DateTimeOffset.UtcNow };
            db.BioProfiles.Add(profile);
            await db.SaveChangesAsync(ct);
        }
        return profile;
    }

    public async Task SaveProfileAsync(BioProfile profile, CancellationToken ct = default)
    {
        profile.UpdatedAt = DateTimeOffset.UtcNow;
        if (profile.Id == 0) db.BioProfiles.Add(profile);
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> SaveEntryAsync(BioEntry entry, CancellationToken ct = default)
    {
        if (entry.Id == 0) db.BioEntries.Add(entry);
        else db.BioEntries.Update(entry);
        await db.SaveChangesAsync(ct);
        return entry.Id;
    }

    public async Task DeleteEntryAsync(int entryId, CancellationToken ct = default)
    {
        var e = await db.BioEntries.FirstOrDefaultAsync(x => x.Id == entryId, ct);
        if (e is null) return;
        db.BioEntries.Remove(e);
        await db.SaveChangesAsync(ct);
    }
}
