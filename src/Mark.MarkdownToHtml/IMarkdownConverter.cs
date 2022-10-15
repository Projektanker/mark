namespace Mark.MarkdownToHtml
{
    public interface IMarkdownConverter
    {
        Task<HtmlDocument> ToHtml(MarkdownToHtmlJob job);
    }
}