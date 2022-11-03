using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mark.Web.Pages;

public abstract class DefaultPageModel : PageModel
{
    public abstract string Title { get; }
}
