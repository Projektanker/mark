namespace Mark.HtmlToPdf;

public class PrintJob
{
    public PrintJob(string documentHtml, string? headerHtml, string? footerHtml, PrintJobPageLayout pageLayout)
    {
        DocumentHtml = documentHtml;
        HeaderHtml = headerHtml;
        FooterHtml = footerHtml;
        PageLayout = pageLayout;
    }

    public bool PrintHeaderAndFooter => HeaderHtml != null || FooterHtml != null;

    public string DocumentHtml { get; }
    public string? HeaderHtml { get; }
    public string? FooterHtml { get; }
    public PrintJobPageLayout PageLayout { get; }
}