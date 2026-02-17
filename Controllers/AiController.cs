using Microsoft.AspNetCore.Mvc;
using ConfigService.Data;
using ConfigService.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfigService.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly ConfigContext _context;

    public AiController(ConfigContext context)
    {
        _context = context;
    }

    private bool IsAuthorized()
    {
        if (!Request.Headers.TryGetValue("X-System-Key", out var headerKey))
        {
            return false;
        }

        var systemKey = _context.SystemOptions
            .Where(o => o.Key == "GlobalAiApiKey")
            .Select(o => o.Value)
            .FirstOrDefault();

        return systemKey == headerKey.ToString();
    }

    [HttpGet("apps")]
    public async Task<IActionResult> GetApps()
    {
        if (!IsAuthorized()) return Unauthorized("Invalid X-System-Key");

        var apps = await _context.Applications.ToListAsync();
        return Ok(apps);
    }

    [HttpGet("envs/{appId}")]
    public async Task<IActionResult> GetEnvs(int appId)
    {
        if (!IsAuthorized()) return Unauthorized("Invalid X-System-Key");

        var envs = await _context.Environments
            .Where(e => e.ApplicationId == appId)
            .ToListAsync();
        return Ok(envs);
    }

    [HttpGet("config/{envId}")]
    public async Task<IActionResult> GetConfigs(int envId)
    {
        if (!IsAuthorized()) return Unauthorized("Invalid X-System-Key");

        var configs = await _context.ConfigItems
            .Where(c => c.EnvironmentId == envId)
            .ToListAsync();
        return Ok(configs);
    }

    [HttpPost("config")]
    public async Task<IActionResult> UpsertConfig([FromBody] ConfigItemDto dto)
    {
        if (!IsAuthorized()) return Unauthorized("Invalid X-System-Key");

        var existing = await _context.ConfigItems
            .FirstOrDefaultAsync(c => c.EnvironmentId == dto.EnvId && c.Key == dto.Key);

        if (existing != null)
        {
            existing.Value = dto.Value;
        }
        else
        {
            _context.ConfigItems.Add(new ConfigItem
            {
                EnvironmentId = dto.EnvId,
                Key = dto.Key,
                Value = dto.Value
            });
        }

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Configuration saved" });
    }

    [HttpDelete("config/{id}")]
    public async Task<IActionResult> DeleteConfig(int id)
    {
        if (!IsAuthorized()) return Unauthorized("Invalid X-System-Key");

        var config = await _context.ConfigItems.FindAsync(id);
        if (config == null) return NotFound();

        _context.ConfigItems.Remove(config);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Configuration deleted" });
    }
    [HttpPost("apps")]
    public async Task<IActionResult> CreateApp([FromBody] AppDto dto)
    {
        if (!IsAuthorized()) return Unauthorized("Invalid X-System-Key");

        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("App Name required");

        if (await _context.Applications.AnyAsync(a => a.Name == dto.Name))
        {
            return Conflict("App with this name already exists");
        }

        var app = new Application
        {
            Name = dto.Name,
            ApiKey = Guid.NewGuid().ToString("N")
        };

        _context.Applications.Add(app);
        await _context.SaveChangesAsync();

        return Ok(app);
    }

    [HttpPost("envs")]
    public async Task<IActionResult> CreateEnv([FromBody] EnvDto dto)
    {
        if (!IsAuthorized()) return Unauthorized("Invalid X-System-Key");

        if (string.IsNullOrWhiteSpace(dto.AppName) || string.IsNullOrWhiteSpace(dto.EnvName))
            return BadRequest("AppName and EnvName required");

        var app = await _context.Applications.FirstOrDefaultAsync(a => a.Name == dto.AppName);
        if (app == null) return NotFound($"App '{dto.AppName}' not found");

        if (await _context.Environments.AnyAsync(e => e.ApplicationId == app.Id && e.Name == dto.EnvName))
        {
             return Conflict($"Environment '{dto.EnvName}' already exists for App '{dto.AppName}'");
        }

        var env = new ConfigService.Models.Environment
        {
            Name = dto.EnvName,
            ApplicationId = app.Id
        };

        _context.Environments.Add(env);
        await _context.SaveChangesAsync();

        return Ok(env);
    }
}

public class ConfigItemDto
{
    public int EnvId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class AppDto
{
    public string Name { get; set; } = string.Empty;
}

public class EnvDto
{
    public string AppName { get; set; } = string.Empty;
    public string EnvName { get; set; } = string.Empty;
}
