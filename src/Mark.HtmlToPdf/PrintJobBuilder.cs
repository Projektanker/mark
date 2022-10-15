using YamlDotNet.Serialization;

namespace Mark.HtmlToPdf;

public class PrintJobBuilder
{
    private PrintJobBuilder(Func<Task<string>> readDocumentHtml)
    {
        ReadDocumentHtml = readDocumentHtml;
    }

    private Func<Task<string>> ReadDocumentHtml { get; set; }
    private Func<Task<string>>? ReadHeaderHtml { get; set; }
    private Func<Task<string>>? ReadFooterHtml { get; set; }
    private Func<Task<PrintJobPageLayout>>? ReadPageLayout { get; set; }

    public static PrintJobBuilder FromFile(string path)
    {
        return new PrintJobBuilder(() => File.ReadAllTextAsync(path));
    }

    public static PrintJobBuilder FromHtml(string html)
    {
        return new PrintJobBuilder(() => Task.FromResult(html));
    }

    public PrintJobBuilder WithHeaderFile(string? path)
    {
        if (path != null)
        {
            ReadHeaderHtml = () => File.ReadAllTextAsync(path);
        }

        return this;
    }

    public PrintJobBuilder WithFooterFile(string? path)
    {
        if (path != null)
        {
            ReadFooterHtml = () => File.ReadAllTextAsync(path);
        }
        return this;
    }

    public PrintJobBuilder WithPageLayout(PrintJobPageLayout pageLayout)
    {
        ReadPageLayout = () => Task.FromResult(pageLayout);
        return this;
    }

    public PrintJobBuilder WithPageLayoutYamlFile(string? yamlFile)
    {
        if (yamlFile != null)
        {
            ReadPageLayout = () => GetPageLayoutFromYamlFile(yamlFile);
        }

        return this;
    }

    public async Task<PrintJob> ToPrintJob()
    {
        return new PrintJob(
            documentHtml: await ReadDocumentHtml(),
            headerHtml: ReadHeaderHtml == null ? null : await ReadHeaderHtml(),
            footerHtml: ReadFooterHtml == null ? null : await ReadFooterHtml(),
            pageLayout: ReadPageLayout == null ? PrintJobPageLayout.Default : await ReadPageLayout());
    }

    private static async Task<PrintJobPageLayout> GetPageLayoutFromYamlFile(string yamlFile)
    {
        var input = await File.ReadAllTextAsync(yamlFile);
        var deserializer = new Deserializer();
        var values = deserializer.Deserialize<Dictionary<string, string>>(input);
        var lookup = new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);
        return new PrintJobPageLayout(
            Format: lookup.GetValueOrDefault("format", PrintJobPageLayout.Default.Format),
            Top: lookup.GetValueOrDefault("top", PrintJobPageLayout.Default.Top),
            Left: lookup.GetValueOrDefault("left", PrintJobPageLayout.Default.Left),
            Right: lookup.GetValueOrDefault("right", PrintJobPageLayout.Default.Right),
            Bottom: lookup.GetValueOrDefault("bottom", PrintJobPageLayout.Default.Bottom));
    }
}