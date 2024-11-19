using Admin.Application.Common.Models;

namespace Admin.Application.Common.Interfaces;
public interface IFileStorage
{
    Task<FileUploadResult> UploadAsync(
        FileUploadRequest file,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);
}
