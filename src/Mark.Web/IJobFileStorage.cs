namespace Mark.Web
{
    public interface IJobFileStorage
    {
        Task WriteFile(Guid jobId, string name, Stream stream);

        string? GetFilePathIfExists(Guid jobId, string name);
    }
}