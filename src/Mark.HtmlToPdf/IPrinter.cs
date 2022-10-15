namespace Mark.HtmlToPdf;

public interface IPrinter
{
    Task<PdfDocument> PrintToPdf(PrintJob printJob);
}
