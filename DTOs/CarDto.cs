using System.ComponentModel.DataAnnotations;

namespace CarApi.DTOs;

public class CarCreateRequest
{
    [Required]
    [StringLength(50)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [Range(1900, 2030)]
    public int Year { get; set; }

    [Required]
    [Range(0, 1000000)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(30)]
    public string Color { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
}

public class CarResponse
{
    public string Id { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string Color { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
