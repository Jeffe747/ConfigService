using ConfigService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddOpenApi();
builder.Services.AddScoped<ConfigService.Services.IConfigSubstitutionService, ConfigService.Services.ConfigSubstitutionService>();


// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var provider = builder.Configuration.GetValue<string>("DatabaseProvider");

builder.Services.AddDbContext<ConfigContext>(options =>
{
    if (provider?.Equals("MSSQL", StringComparison.OrdinalIgnoreCase) == true)
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        // Default to SQLite
        var dbPath = Path.Join(builder.Environment.ContentRootPath, "config.db");
        
        var dataDir = builder.Configuration.GetValue<string>("DataDirectory");
        if (!string.IsNullOrEmpty(dataDir))
        {
            Directory.CreateDirectory(dataDir);
            dbPath = Path.Combine(dataDir, "config.db");
        }

        options.UseSqlite($"Data Source={dbPath}");
    }
});

var app = builder.Build();

// Migrate Database on Startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ConfigContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run("http://*:5001");
