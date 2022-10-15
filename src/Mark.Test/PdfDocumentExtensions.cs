using Mark.HtmlToPdf;

namespace Mark.Test;

public static class PdfDocumentExtensions
{
    public static void ShouldContain(this PdfDocument pdfDocument, string text)
    {
        using var doc = UglyToad.PdfPig.PdfDocument.Open(pdfDocument.GetContentStream());
        doc.ShouldContain(text);
    }
}