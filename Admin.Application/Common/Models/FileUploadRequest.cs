namespace Admin.Application.Common.Models;
public class FileUploadRequest
{
    public string FileName { get; init; } = string.Empty;
    public long Length { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public Stream Content { get; init; } = Stream.Null;
    public string Url { get; set; } = string.Empty;
}
