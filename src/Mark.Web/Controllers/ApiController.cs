using System.Net;
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
    [SwaggerResponse((int)HttpStatusCode.OK, "Download a file.", typeof(FileStreamResult))]
    public async Task<IActionResult> Pdf(IFormFileCollection files)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var jobId = Guid.NewGuid();
        foreach (var file in files)
        {
            using var stream = file.OpenReadStream();
            await _jobFileStorage.WriteFile(jobId, file.FileName, stream);
        }

        // 2. Generate DocumentJob
        const string templateFilename = "template.md";
        const string metadataFilename = "metadata.yaml";
        var markdownTemplateFile = _jobFileStorage.GetFilePathIfExists(jobId, templateFilename);
        var metadataFile = _jobFileStorage.GetFilePathIfExists(jobId, metadataFilename);

        if (markdownTemplateFile == null)
        {
            return BadRequest($"Required file \"{templateFilename}\" is missing.");
        }

        if (metadataFile == null)
        {
            return BadRequest($"Required file \"{metadataFilename}\" is missing.");
        }

        var markdownToHtmlJob = new MarkdownToHtmlJob(
            markdownTemplateFile: markdownTemplateFile,
            metadataFile: metadataFile,
            cssFile: _jobFileStorage.GetFilePathIfExists(jobId, "document.css"),
            htmlTemplateFile: _jobFileStorage.GetFilePathIfExists(jobId, "template.html"));

        var htmlDocument = await _markdownConverter.ToHtml(markdownToHtmlJob);

        var printJob = await PrintJobBuilder.FromHtml(htmlDocument.Html)
            .WithHeaderFile(_jobFileStorage.GetFilePathIfExists(jobId, "header.html"))
            .WithFooterFile(_jobFileStorage.GetFilePathIfExists(jobId, "footer.html"))
            .WithPageLayoutYamlFile(_jobFileStorage.GetFilePathIfExists(jobId, "pagelayout.yaml"))
            .ToPrintJob();

        // 3. Generate Pdf
        var pdf = await _printer.PrintToPdf(printJob);

        return File(pdf.GetContentStream(), "application/pdf");
    }
}