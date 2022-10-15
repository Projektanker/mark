using Mark.HtmlToPdf;
using PdfPigDoc = UglyToad.PdfPig.PdfDocument;

namespace Mark.Test.HtmlToPdf;

public class PrinterTest
{
    private static readonly string Resources = $"{nameof(HtmlToPdf)}/resrc/";
    private readonly IPrinter _printer = new Printer();

    [Fact]
    public async Task Print_Without_Header_And_Footer()
    {
        const string outputFile = "printer-without-header-footer.pdf";
        FileDumper.EnsureDeleted(outputFile);

        var job = await PrintJobBuilder
            .FromFile(Resources + "input.html")
            .ToPrintJob();

        var pdf = await _printer.PrintToPdf(job);
        FileDumper.Write(outputFile, pdf.GetContentStream());
        pdf.ShouldContain("Lorem ipsum");
    }

    [Fact]
    public async Task Print_Whith_Header_And_Footer()
    {
        const string outputFile = "printer-with-header-footer.pdf";
        FileDumper.EnsureDeleted(outputFile);

        var job = await PrintJobBuilder
            .FromFile(Resources + "input.html")
            .WithHeaderFile(Resources + "header.html")
            .WithFooterFile(Resources + "footer.html")
            .WithPageLayout(PrintJobPageLayout.Default with { Top = "25mm", Bottom = "25mm" })
            .ToPrintJob();

        var pdf = await _printer.PrintToPdf(job);
        FileDumper.Write(outputFile, pdf.GetContentStream());

        pdf.ShouldContain("Lorem ipsum");
        pdf.ShouldContain("Page 1 of");
        pdf.ShouldContain("MARK");
    }
}