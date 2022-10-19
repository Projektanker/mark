using System.Net;
using System.Text;
using Mark.HtmlToPdf;
using Mark.MarkdownToHtml;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Mark.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{
    private readonly IJobFileStorage _jobFileStorage;
    private readonly IMarkdownConverter _markdownConverter;
    private readonly IPrinter _printer;

    public ApiController(IJobFileStorage jobFileStorage, IMarkdownConverter markdownConverter, IPrinter printer)
    {
        _jobFileStorage = jobFileStorage;
        _markdownConverter = markdownConverter;
        _printer = printer;
    }

    [HttpPost("pdf")]
    [SwaggerResponse((int)HttpStatusCode.OK, "pdf file", typeof(FileStreamResult))]
    public async Task<IActionResult> Pdf(IFormFileCollection files)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var jobId = Guid.NewGuid();
        var (htmlDocument, error) = await GenerateHtml(jobId, files);

        if (htmlDocument is null)
        {
            return BadRequest(error);
        }

        PdfDocument pdf = await GeneratePdf(jobId, htmlDocument);

        return File(pdf.GetContentStream(), "application/pdf");
    }

    [HttpPost("html")]
    [SwaggerResponse((int)HttpStatusCode.OK, "debug html file", typeof(FileStreamResult))]
    public async Task<IActionResult> Html(IFormFileCollection files)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var jobId = Guid.NewGuid();
        var (htmlDocument, error) = await GenerateHtml(jobId, files);

        if (htmlDocument is null)
        {
            return BadRequest(error);
        }

        return File(Encoding.UTF8.GetBytes(htmlDocument.Html), "text/html");
    }

    private async Task<(HtmlDocument? Html, string Error)> GenerateHtml(Guid jobId, IFormFileCollection files)
    {
        foreach (var file in files)
        {
            using var stream = file.OpenReadStream();
            await _jobFileStorage.WriteFile(jobId, file.FileName, stream);
        }

        const string templateFilename = "template.md";
        const string metadataFilename = "metadata.yaml";
        var markdownTemplateFile = _jobFileStorage.GetFilePathIfExists(jobId, templateFilename);
        var metadataFile = _jobFileStorage.GetFilePathIfExists(jobId, metadataFilename);

        if (markdownTemplateFile == null)
        {
            return (null, $"Required file \"{templateFilename}\" is missing.");
        }

        if (metadataFile == null)
        {
            return (null, $"Required file \"{metadataFilename}\" is missing.");
        }

        var markdownToHtmlJob = new MarkdownToHtmlJob(
            markdownTemplateFile: markdownTemplateFile,
            metadataFile: metadataFile,
            cssFile: _jobFileStorage.GetFilePathIfExists(jobId, "document.css"),
            htmlTemplateFile: _jobFileStorage.GetFilePathIfExists(jobId, "template.html"));

        var htmlDocument = await _markdownConverter.ToHtml(markdownToHtmlJob);
        return (htmlDocument, string.Empty);
    }

    private async Task<PdfDocument> GeneratePdf(Guid jobId, HtmlDocument htmlDocument)
    {
        var printJob = await PrintJobBuilder.FromHtml(htmlDocument.Html)
            .WithHeaderFile(_jobFileStorage.GetFilePathIfExists(jobId, "header.html"))
            .WithFooterFile(_jobFileStorage.GetFilePathIfExists(jobId, "footer.html"))
            .WithPageLayoutYamlFile(_jobFileStorage.GetFilePathIfExists(jobId, "pagelayout.yaml"))
            .ToPrintJob();

        var pdf = await _printer.PrintToPdf(printJob);
        return pdf;
    }
}