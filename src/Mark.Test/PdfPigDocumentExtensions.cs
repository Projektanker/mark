using UglyToad.PdfPig;

namespace Mark.Test;

public static class PdfPigDocumentExtensions
{
    public static string GetText(this PdfDocument document)
    {
        var pageTexts = document.GetPages()
            .Select(page => page.Text);

        var separator = Environment.NewLine + Environment.NewLine;
        return string.Join(separator, pageTexts);
    }

    public static void ShouldContain(this PdfDocument document, string text)
        => document.GetText().Should().Contain(text);
}
