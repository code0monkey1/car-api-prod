using CarApi.Models;

namespace CarApi.Services;

public interface IGoogleSheetsService
{
    Task AppendCarAsync(Car car);
    Task<List<Car>> GetAllCarsAsync();
    Task ClearCarsAsync();
}
