using CarApi.Services;
using Microsoft.OpenApi.Models;

// Load environment variables from Render secret file
if (File.Exists("/etc/secrets/.env.render"))
{
    var envFile = File.ReadAllText("/etc/secrets/.env.render");
    foreach (var line in envFile.Split('\n', StringSplitOptions.RemoveEmptyEntries))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
            Environment.SetEnvironmentVariable(parts[0], parts[1]);
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Car API",
        Version = "v1",
        Description = "A REST API for managing car records with Google Sheets & Drive integration",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        }
    });
});

// Register Google Services
builder.Services.AddSingleton<IGoogleSheetsService, GoogleSheetsService>();
builder.Services.AddSingleton<IGoogleDriveService, GoogleDriveService>();

// CORS - Allow all origins for public API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ENABLE_SWAGGER") == "true")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Car API V1");
        c.RoutePrefix = string.Empty; // Swagger UI at root
    });
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
