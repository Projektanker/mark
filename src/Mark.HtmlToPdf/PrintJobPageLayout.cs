namespace Mark.HtmlToPdf;

public record PrintJobPageLayout(string Format, string Top, string Left, string Right, string Bottom)
{
    private const string DefaultMargin = "10mm";
    public static PrintJobPageLayout Default { get; } = new("A4", DefaultMargin, DefaultMargin, DefaultMargin, DefaultMargin);
}