namespace Admin.Application.Common.Models;
public class FileUploadResult
{
    public string Url { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long Size { get; init; }
}
