using Microsoft.AspNetCore.Mvc.RazorPages;
using Phipes.Blog.Services;

namespace Phipes.Blog.Areas.PhipesBlog.Pages.Admin;

public sealed class IndexModel(
    IBlogService blog, IProjectService projects, ICommentService comments, IContactService contact) : PageModel
{
    public int PostCount { get; private set; }
    public int ProjectCount { get; private set; }
    public int PendingComments { get; private set; }
    public int NewMessages { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        PostCount = (await blog.ListAllAsync(1, 1, ct)).TotalCount;
        ProjectCount = (await projects.ListAllAsync(1, 1, ct)).TotalCount;
        PendingComments = (await comments.GetPendingAsync(ct)).Count;
        NewMessages = await contact.CountByStatusAsync(Domain.ContactMessageStatus.New, ct);
    }
}
