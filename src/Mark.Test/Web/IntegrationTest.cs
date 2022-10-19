using System.IO;
using System.IO.Compression;
using System.Net;
using Mark.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
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

    [Fact]
    public async Task Test_With_TAR_Archive()
    {
        FileDumper.EnsureDeleted("test-with-tar.pdf");

        using var client = _factory.CreateClient();

        var formContent = GetTarFormDataContent();
        formContent.Should().HaveCount(1);

        var response = await client.PostAsync("/api/pdf", formContent);

        await AssertResponse(response, "test-with-tar.pdf");
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

        foreach (var file in EnumerateResources())
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

    private static MultipartFormDataContent GetTarFormDataContent()
    {
        var formDataContent = new MultipartFormDataContent();

        var tarStream = new MemoryStream();
        using var tar = TarArchive.Create();
        foreach (var file in EnumerateResources())
        {
            var fileInfo = new FileInfo(file);
            var key = Path.GetRelativePath(Resources, file);
            tar.AddEntry(key, fileInfo.OpenRead(), true, fileInfo.Length);
        }

        tar.SaveTo(tarStream, new(CompressionType.None));
        var tarContent = new StreamContent(tarStream);
        formDataContent.Add(tarContent, "files", "test.tar");

        return formDataContent;
    }

    private static IEnumerable<string> EnumerateResources()
        => Directory.EnumerateFiles(Resources, "*.*", SearchOption.AllDirectories);
}
