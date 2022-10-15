using System.Diagnostics;
using System.Text;

namespace Mark.MarkdownToHtml;

public class PandocMarkdownConverter : IMarkdownConverter
{
    public async Task<HtmlDocument> ToHtml(MarkdownToHtmlJob job)
    {
        var pandocArgs = job.ToCommandLineArgs();
        var html = await ExecuteCommand(pandocArgs);
        return new(html);
    }

    private static async Task<string> ExecuteCommand(string command)
    {
        var (startInfo, @finally) = await GetProcessStartInfo(command);
        try
        {
            var sb = new StringBuilder();
            var process = new Process()
            {
                EnableRaisingEvents = true,
                StartInfo = startInfo
            };

            process.OutputDataReceived += (_, args) => sb.Append(args.Data);
            process.ErrorDataReceived += (_, args) => sb.Append(args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            return process.ExitCode == 0 ? sb.ToString() : $"Exception: process exit code {process.ExitCode}. {sb}";
        }
        finally
        {
            @finally?.Invoke();
        }
    }

    private static async Task<(ProcessStartInfo, Action?)> GetProcessStartInfo(string command)
    {
        var startInfo = new ProcessStartInfo
        {
            StandardOutputEncoding = Encoding.UTF8,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        Action? @finally = null;

        if (IsWindows())
        {
            startInfo.FileName = "cmd";
            startInfo.Arguments = $"/c {command}";
        }
        else
        {
            var temp = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName());

            await File.WriteAllTextAsync(temp, command);
            @finally = () => File.Delete(temp);

            startInfo.FileName = "bash";
            startInfo.Arguments = temp;
        }

        return (startInfo, @finally);
    }

    private static bool IsWindows() => OperatingSystem.IsWindows();
}