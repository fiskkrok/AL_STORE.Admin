namespace Admin.Application.Common.Models;
public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    public string? StackTrace { get; set; }
}
