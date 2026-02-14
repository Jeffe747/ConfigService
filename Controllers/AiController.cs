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
}

public class ConfigItemDto
{
    public int EnvId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
