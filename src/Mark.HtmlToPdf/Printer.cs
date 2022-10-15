using Microsoft.Playwright;

namespace Mark.HtmlToPdf;

public class Printer : IPrinter
{
    public async Task<PdfDocument> PrintToPdf(PrintJob printJob)
    {
        var playwright = await Playwright.CreateAsync();
        var chromium = playwright.Chromium;

        var browser = await chromium.LaunchAsync();

        var page = await browser.NewPageAsync();
        await page.SetContentAsync(printJob.DocumentHtml, new()
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        var content = await page.PdfAsync(new()
        {
            Format = printJob.PageLayout.Format,
            Margin = GetMargin(printJob.PageLayout),
            DisplayHeaderFooter = printJob.PrintHeaderAndFooter,
            HeaderTemplate = GetTemplate(printJob.HeaderHtml),
            FooterTemplate = GetTemplate(printJob.FooterHtml),
        });

        return new PdfDocument(content);
    }

    private static string? GetTemplate(string? html)
    {
        return html == null ? null : CssInliner.Inline(html);
    }

    private static Margin GetMargin(PrintJobPageLayout margin)
    {
        return new Margin
        {
            Top = margin.Top,
            Left = margin.Left,
            Right = margin.Right,
            Bottom = margin.Bottom,
        };
    }
}