using System.IO.Compression;
using System.Net;
using Mark.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using UglyToad.PdfPig;

namespace Mark.Test.Web;

public class IntegrationTest
{
    private static readonly string Resources = $"{nameof(Web)}/resrc/";
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTest()
    {
        _factory = new WebApplicationFactory<Program>();
    }

    [Fact]
    public async Task Test_Without_ZIP_Archive()
    {
        FileDumper.EnsureDeleted("test-without-zip.pdf");

        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/pdf", GetFormDataContent());
        await AssertResponse(response, "test-without-zip.pdf");
    }

    [Fact]
    public async Task Test_With_ZIP_Archive()
    {
        FileDumper.EnsureDeleted("test-with-zip.pdf");

        using var client = _factory.CreateClient();

        var formContent = GetZippedFormDataContent("metadata.yaml");
        formContent.Should().HaveCount(2);

        var response = await client.PostAsync("/api/pdf", formContent);

        await AssertResponse(response, "test-with-zip.pdf");
    }

    private static async Task AssertResponse(HttpResponseMessage response, string outputFile)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsByteArrayAsync();

        FileDumper.Write(outputFile, content);
        var pdfDoc = PdfDocument.Open(content);

        // Header
        pdfDoc.ShouldContain("MARK");

        // Footer
        pdfDoc.ShouldContain("Page 1 of");

        // Body
        pdfDoc.ShouldContain("This is a test-sub-title");
        pdfDoc.ShouldContain("paragraph");
    }

    private static MultipartFormDataContent GetFormDataContent()
    {
        var formDataContent = new MultipartFormDataContent();
        foreach (var file in Directory.EnumerateFiles(Resources))
        {
            var bytes = File.ReadAllBytes(file);
            var content = new ByteArrayContent(bytes);
            formDataContent.Add(content, "files", Path.GetFileName(file));
        }

        return formDataContent;
    }

    private static MultipartFormDataContent GetZippedFormDataContent(params string[] exclude)
    {
        var formDataContent = new MultipartFormDataContent();

        var zipStream = new MemoryStream();
        using var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true);
        foreach (var file in Directory.EnumerateFiles(Resources))
        {
            var fileName = Path.GetFileName(file);
            if (exclude.Contains(fileName))
            {
                continue;
            }

            zip.CreateEntryFromFile(file, fileName);
        }

        var zipContent = new StreamContent(zipStream);
        formDataContent.Add(zipContent, "files", "test.zip");

        foreach (var file in Directory.EnumerateFiles(Resources))
        {
            var fileName = Path.GetFileName(file);
            if (!exclude.Contains(fileName))
            {
                continue;
            }
            var bytes = File.ReadAllBytes(file);
            var content = new ByteArrayContent(bytes);
            formDataContent.Add(content, "files", fileName);
        }

        return formDataContent;
    }
}
