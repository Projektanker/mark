using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using ZipArchive = SharpCompress.Archives.Zip.ZipArchive;

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
            ".TAR" => WriteTar(jobId, stream),
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
        using var archive = ZipArchive.Open(stream);
        await WriteArchive(jobId, archive);
    }

    private async Task WriteTar(Guid jobId, Stream stream)
    {
        using var archive = TarArchive.Open(stream);
        await WriteArchive(jobId, archive);
    }

    private async Task WriteArchive<TEntry, TVolume>(Guid jobId, AbstractArchive<TEntry, TVolume> archive)
        where TEntry : IArchiveEntry
        where TVolume : IVolume
    {
        var jobDir = GetOrCreateJobDir(jobId);
        var absoluteJobDir = Path.GetFullPath(jobDir);
        var fileEntries = archive.Entries.Where(entry => !entry.IsDirectory);
        foreach (var entry in fileEntries)
        {
            // Gets the full path to ensure that relative segments are removed.
            var destinationPath = Path.GetFullPath(Path.Combine(jobDir, entry.Key));

            // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
            // are case-insensitive.
            if (!destinationPath.StartsWith(absoluteJobDir, StringComparison.Ordinal))
            {
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            using var entryStream = entry.OpenEntryStream();
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