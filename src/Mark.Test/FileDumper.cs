using System.Runtime.CompilerServices;

namespace Mark.Test;

public static class FileDumper
{
    public static void EnsureDeleted(string filename, [CallerFilePath] string? callerFilePath = null)
    {
        var filePath = GetFilePath(filename, callerFilePath!);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public static void Write(string filename, byte[] content, [CallerFilePath] string? callerFilePath = null)
    {
        var filePath = GetFilePath(filename, callerFilePath!);
        File.WriteAllBytes(filePath, content);
    }

    internal static void Write(string filename, Stream stream, [CallerFilePath] string? callerFilePath = null)
    {
        var filePath = GetFilePath(filename, callerFilePath!);
        using var file = File.Create(filePath);
        stream.CopyTo(file);
    }

    private static string GetDirectoryForCaller(string callerFilePath)
    {
        var callerDir = new DirectoryInfo(Path.GetDirectoryName(callerFilePath)!);
        var targetDir = Path.Combine("_test", callerDir.Name);
        Directory.CreateDirectory(targetDir);
        return targetDir;
    }

    private static string GetFilePath(string filename, string callerFilePath)
    {
        var dir = GetDirectoryForCaller(callerFilePath!);
        return Path.Combine(dir, filename);
    }
}
