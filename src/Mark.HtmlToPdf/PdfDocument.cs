namespace Mark.HtmlToPdf;

public class PdfDocument
{
    private readonly byte[] _content;

    public PdfDocument(byte[] content)
    {
        _content = content;
    }

    public Stream GetContentStream() => new MemoryStream(_content);
}