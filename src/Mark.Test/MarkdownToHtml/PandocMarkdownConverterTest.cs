using Mark.MarkdownToHtml;

namespace Mark.Test.MarkdownToHtml;

public class PandocMarkdownConverterTest
{
    private static readonly string Resources = $"{nameof(MarkdownToHtml)}/resrc/";
    private readonly PandocMarkdownConverter _pandoc = new();

    [Fact]
    public async Task Converts_Markdown_To_HtmlAsync()
    {
        var job = new MarkdownToHtmlJob(
            markdownTemplateFile: Resources + "template.md",
            metadataFile: Resources + "metadata.yaml",
            cssFile: null,
            htmlTemplateFile: null);

        AssertJob(job);

        var htmlDoc = await _pandoc.ToHtml(job);

        htmlDoc.Html.Should().StartWith("<!DOCTYPE html>");
        htmlDoc.Html.Should().Contain("<title>test-title</title>");
        htmlDoc.Html.Should().Contain("This is a test-sub-title");
        htmlDoc.Html.Should().Contain("<p>paragraph</p>");
    }

    [Fact]
    public async Task Converts_Markdown_To_HtmlAsync_With_Included_CSS()
    {
        var job = new MarkdownToHtmlJob(
            markdownTemplateFile: Resources + "template.md",
            metadataFile: Resources + "metadata.yaml",
            cssFile: Resources + "document.css",
            htmlTemplateFile: null);

        var htmlDoc = await _pandoc.ToHtml(job);

        htmlDoc.Html.Should().StartWith("<!DOCTYPE html>");
        htmlDoc.Html.Should().Contain("<title>test-title</title>");
        htmlDoc.Html.Should().Contain(".a-test-class");
    }

    [Fact]
    public async Task Converts_Markdown_To_HtmlAsync_With_Template()
    {
        var job = new MarkdownToHtmlJob(
            markdownTemplateFile: Resources + "template.md",
            metadataFile: Resources + "metadata.yaml",
            cssFile: null,
            htmlTemplateFile: Resources + "template.html");

        var htmlDoc = await _pandoc.ToHtml(job);

        htmlDoc.Html.Should().Contain("<meta template=\"test-template\" />");
        htmlDoc.Html.Should().Contain("This is a test-sub-title");
    }

    private static void AssertJob(MarkdownToHtmlJob job)
    {
        File.Exists(job.MarkdownTemplateFile).Should().BeTrue();
        File.Exists(job.MetadataFile).Should().BeTrue();
    }
}