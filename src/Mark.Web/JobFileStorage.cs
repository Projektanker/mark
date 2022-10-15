using System.IO.Compression;

namespace Mark.Web;

public sealed class JobFileStorage : IJobFileStorage, IDisposable
{
    private static readonly string _baseDir = Path.Combine("mark", "jobs");

    private readonly HashSet<Guid> _jobIds = new();

    public async Task WriteFile(Guid jobId, string name, Stream stream)
    {
        var fileExtension = Path.GetExtension(name).ToUpperInvariant();
        var task = fileExtension switch
        {
            ".ZIP" => WriteZip(jobId, stream),
            _ => WriteNonArchiveFile(jobId, name, stream),
        };

        await task;
    }

    public string? GetFilePathIfExists(Guid jobId, string name)
    {
        var path = GetFilePath(jobId, name);
        return File.Exists(path) ? path : null;
    }

    public void Dispose()
    {
        foreach (var jobId in _jobIds)
        {
            Directory.Delete(GetOrCreateJobDir(jobId), true);
        }
    }

    private async Task WriteNonArchiveFile(Guid jobId, string name, Stream stream)
    {
        using var file = File.Create(GetFilePath(jobId, name));
        await stream.CopyToAsync(file);
    }

    private async Task WriteZip(Guid jobId, Stream stream)
    {
        var jobDir = GetOrCreateJobDir(jobId);
        var absoluteJobDir = Path.GetFullPath(jobDir);

        using var archive = new ZipArchive(stream);

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            // Gets the full path to ensure that relative segments are removed.
            var destinationPath = Path.GetFullPath(Path.Combine(jobDir, entry.FullName));

            // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
            // are case-insensitive.
            if (!destinationPath.StartsWith(absoluteJobDir, StringComparison.Ordinal))
            {
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            using var entryStream = entry.Open();
            using var file = File.Create(destinationPath);
            await entryStream.CopyToAsync(file);
        }
    }

    private string GetOrCreateJobDir(Guid jobId)
    {
        _jobIds.Add(jobId);
        var jobDir = Path.Combine(_baseDir, jobId.ToString("N"));
        Directory.CreateDirectory(jobDir);
        return jobDir;
    }

    private string GetFilePath(Guid jobId, string name)
    {
        return Path.Combine(GetOrCreateJobDir(jobId), name);
    }
}