using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using CarApi.Models;

namespace CarApi.Services;

public class GoogleSheetsService : IGoogleSheetsService
{
    private readonly string _spreadsheetId;
    private readonly SheetsService _sheetsService;
    private readonly string _readRange = "Cars!A:H";
    private readonly string _appendRange = "Cars!A:G"; // Append range must be narrower than data columns to anchor at column A

    public GoogleSheetsService(IConfiguration configuration)
    {
        _spreadsheetId = configuration["GoogleSheets:SpreadsheetId"] 
            ?? throw new ArgumentNullException("GoogleSheets:SpreadsheetId configuration is required");

        var credentialJson = configuration["GoogleSheets:CredentialsJson"]
            ?? throw new ArgumentNullException("GoogleSheets:CredentialsJson configuration is required");

        var credential = GoogleCredential.FromJson(credentialJson)
            .CreateScoped(SheetsService.Scope.Spreadsheets);

        _sheetsService = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "CarApi"
        });
    }

    public async Task AppendCarAsync(Car car)
    {
        var values = new List<IList<object>>
        {
            new List<object>
            {
                car.Id,
                car.Make,
                car.Model,
                car.Year,
                car.Price,
                car.Color,
                car.ImageUrl ?? "",
                car.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            }
        };

        var request = new Google.Apis.Sheets.v4.Data.ValueRange
        {
            Values = values
        };

        var appendRequest = _sheetsService.Spreadsheets.Values.Append(
            request,
            _spreadsheetId,
            _appendRange);

        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        
        await appendRequest.ExecuteAsync();
    }

    public async Task<List<Car>> GetAllCarsAsync()
    {
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, _readRange);
        var response = await request.ExecuteAsync();

        var cars = new List<Car>();

        if (response.Values == null || response.Values.Count <= 1)
            return cars;

        // Skip header row
        foreach (var row in response.Values.Skip(1))
        {
            if (row.Count < 6 || string.IsNullOrWhiteSpace(row[0]?.ToString())) continue;

            cars.Add(new Car
            {
                Id = row[0]?.ToString() ?? "",
                Make = row[1]?.ToString() ?? "",
                Model = row[2]?.ToString() ?? "",
                Year = int.TryParse(row[3]?.ToString(), out var year) ? year : 0,
                Price = decimal.TryParse(row[4]?.ToString(), out var price) ? price : 0,
                Color = row[5]?.ToString() ?? "",
                ImageUrl = row.Count > 6 ? row[6]?.ToString() ?? "" : "",
                CreatedAt = row.Count > 7 && DateTime.TryParse(row[7]?.ToString(), out var createdAt) ? createdAt : DateTime.UtcNow
            });
        }

        return cars;
    }

    public async Task ClearCarsAsync()
    {
        var clearRequest = _sheetsService.Spreadsheets.Values.Clear(
            new Google.Apis.Sheets.v4.Data.ClearValuesRequest(),
            _spreadsheetId,
            "Cars!A2:H1000");

        await clearRequest.ExecuteAsync();
    }
}
