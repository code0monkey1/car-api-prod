namespace CarApi.Services;

public interface IGoogleDriveService
{
    Task<string?> UploadFileAsync(Stream fileStream, string fileName, string mimeType);
}
