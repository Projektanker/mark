using Microsoft.AspNetCore.Razor.TagHelpers;
using Projektanker.RazorComponents;

namespace Mark.Web.Pages.Components;

[HtmlTargetElement(nameof(MultiFileInput))]
public class MultiFileInput : RazorComponentTagHelper
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? XModel { get; set; }
}
