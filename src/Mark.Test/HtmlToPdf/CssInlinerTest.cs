using Mark.HtmlToPdf;

namespace Mark.Test.HtmlToPdf;

public class CssInlinerTest
{
    private static readonly string Resources = $"{nameof(HtmlToPdf)}/resrc/";

    [Fact]
    public void Inlines_Css()
    {
        var html = File.ReadAllText(Resources + "footer.html");
        var inlined = CssInliner.Inline(html);
        var expected = File.ReadAllText(Resources + "footer-inline.html");
        Assert(inlined, expected);
    }

    private static void Assert(string inlined, string expected)
    {
        inlined = inlined.Replace("\r\n", "\n");
        expected = expected.Replace("\r\n", "\n");
        inlined.Should().Be(expected);
    }
}