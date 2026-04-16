using Microsoft.AspNetCore.Mvc;
using CarApi.DTOs;
using CarApi.Models;
using CarApi.Services;

namespace CarApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarsController : ControllerBase
{
    private readonly IGoogleSheetsService _sheetsService;
    private readonly IGoogleDriveService _driveService;
    private readonly ILogger<CarsController> _logger;

    public CarsController(
        IGoogleSheetsService sheetsService,
        IGoogleDriveService driveService,
        ILogger<CarsController> logger)
    {
        _sheetsService = sheetsService;
        _driveService = driveService;
        _logger = logger;
    }

    /// <summary>
    /// Add a new car record to Google Sheets with optional image upload to Drive
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CarResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CarResponse>> CreateCar([FromForm] CarCreateRequest request, IFormFile? image = null)
    {
        try
        {
            var car = new Car
            {
                Make = request.Make,
                Model = request.Model,
                Year = request.Year,
                Price = request.Price,
                Color = request.Color,
                ImageUrl = request.ImageUrl
            };

            // Upload image to Google Drive if a file is provided (overrides ImageUrl)
            if (image != null && image.Length > 0)
            {
                using var stream = image.OpenReadStream();
                car.ImageUrl = await _driveService.UploadFileAsync(stream, image.FileName, image.ContentType);
                _logger.LogInformation("Uploaded car image to Drive: {ImageUrl}", car.ImageUrl);
            }

            // Save to Google Sheets
            await _sheetsService.AppendCarAsync(car);

            var response = new CarResponse
            {
                Id = car.Id,
                Make = car.Make,
                Model = car.Model,
                Year = car.Year,
                Price = car.Price,
                Color = car.Color,
                ImageUrl = car.ImageUrl,
                CreatedAt = car.CreatedAt
            };

            return CreatedAtAction(nameof(GetCars), new { }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating car record");
            return BadRequest(new { error = "Failed to create car record", details = ex.Message });
        }
    }

    /// <summary>
    /// Query car records with optional LINQ filtering
    /// </summary>
    /// <param name="make">Filter by car make (case-insensitive)</param>
    /// <param name="minYear">Filter by minimum year</param>
    /// <param name="maxYear">Filter by maximum year</param>
    /// <param name="minPrice">Filter by minimum price</param>
    /// <param name="maxPrice">Filter by maximum price</param>
    /// <param name="color">Filter by color (case-insensitive)</param>
    /// <param name="limit">Limit number of results (default: 100)</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CarResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CarResponse>>> GetCars(
        [FromQuery] string? make = null,
        [FromQuery] int? minYear = null,
        [FromQuery] int? maxYear = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? color = null,
        [FromQuery] int limit = 100)
    {
        try
        {
            var cars = await _sheetsService.GetAllCarsAsync();

            // LINQ filtering
            var query = cars.AsQueryable();

            if (!string.IsNullOrWhiteSpace(make))
            {
                query = query.Where(c => c.Make.Equals(make, StringComparison.OrdinalIgnoreCase));
            }

            if (minYear.HasValue)
            {
                query = query.Where(c => c.Year >= minYear.Value);
            }

            if (maxYear.HasValue)
            {
                query = query.Where(c => c.Year <= maxYear.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(c => c.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(c => c.Price <= maxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(color))
            {
                query = query.Where(c => c.Color.Equals(color, StringComparison.OrdinalIgnoreCase));
            }

            var filteredCars = query.Take(limit).ToList();

            var response = filteredCars.Select(c => new CarResponse
            {
                Id = c.Id,
                Make = c.Make,
                Model = c.Model,
                Year = c.Year,
                Price = c.Price,
                Color = c.Color,
                ImageUrl = c.ImageUrl,
                CreatedAt = c.CreatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving car records");
            return StatusCode(500, new { error = "Failed to retrieve car records", details = ex.Message });
        }
    }

    /// <summary>
    /// Clear all car records from Google Sheets (keeps header row)
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCars()
    {
        try
        {
            await _sheetsService.ClearCarsAsync();
            return Ok(new { message = "All car records cleared" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing car records");
            return StatusCode(500, new { error = "Failed to clear car records", details = ex.Message });
        }
    }
}
