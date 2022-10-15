namespace Mark.MarkdownToHtml;

public class MarkdownToHtmlJob
{
    public MarkdownToHtmlJob(string markdownTemplateFile, string metadataFile, string? cssFile, string? htmlTemplateFile)
    {
        MarkdownTemplateFile = markdownTemplateFile;
        CssFile = cssFile;
        MetadataFile = metadataFile;
        HtmlTemplateFile = htmlTemplateFile;
    }

    public string MarkdownTemplateFile { get; }
    public string MetadataFile { get; }
    public string? CssFile { get; }
    public string? HtmlTemplateFile { get; }

    public string ToCommandLineArgs()
    {
        var args = new List<string?>
        {
            // Markdown template to markdown
            "echo \"\" | pandoc",
            $"--template=\"{MarkdownTemplateFile}\"",
            $"--metadata-file=\"{MetadataFile}\"",
            // Markdown to html
            "| pandoc --standalone --embed-resources",
            $"--metadata-file=\"{MetadataFile}\"",
            CssFile != null ? $"--css=\"{CssFile}\"" : null,
            HtmlTemplateFile != null ? $"--template=\"{HtmlTemplateFile}\"" : null,
            "--to=html5"
        };

        return string.Join(' ', args.Where(arg => arg != null));
    }
}