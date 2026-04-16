using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using File = Google.Apis.Drive.v3.Data.File;

namespace CarApi.Services;

public class GoogleDriveService : IGoogleDriveService
{
    private readonly DriveService _driveService;
    private readonly string _folderId;

    public GoogleDriveService(IConfiguration configuration)
    {
        _folderId = configuration["GoogleDrive:FolderId"] 
            ?? throw new ArgumentNullException("GoogleDrive:FolderId configuration is required");

        var credentialJson = configuration["GoogleDrive:CredentialsJson"]
            ?? throw new ArgumentNullException("GoogleDrive:CredentialsJson configuration is required");

        var credential = GoogleCredential.FromJson(credentialJson)
            .CreateScoped(DriveService.Scope.Drive);

        _driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "CarApi"
        });
    }

    public async Task<string?> UploadFileAsync(Stream fileStream, string fileName, string mimeType)
    {
        var fileMetadata = new File
        {
            Name = fileName,
            Parents = new List<string> { _folderId }
        };

        var request = _driveService.Files.Create(fileMetadata, fileStream, mimeType);
        request.Fields = "id, webViewLink";
        request.SupportsAllDrives = true;

        var response = await request.UploadAsync();

        if (response.Status == Google.Apis.Upload.UploadStatus.Failed)
        {
            throw response.Exception;
        }

        return request.ResponseBody?.WebViewLink;
    }
}
