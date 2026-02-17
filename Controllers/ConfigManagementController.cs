using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConfigService.Data;
using ConfigService.Models;

namespace ConfigService.Controllers;

[ApiController]
[Route("api/apps/{appName}/envs/{envName}/config")]
public class ConfigManagementController : ControllerBase
{
    private readonly ConfigContext _context;

    public ConfigManagementController(ConfigContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConfigItem>>> GetConfigs(string appName, string envName)
    {
        var env = await _context.Environments
            .Include(e => e.ConfigItems)
            .FirstOrDefaultAsync(e => e.Name == envName && e.Application.Name == appName);
            
        if (env == null) return NotFound();

        return env.ConfigItems;
    }

    [HttpPost]
    public async Task<ActionResult<ConfigItem>> UpsertConfig(string appName, string envName, ConfigItem item)
    {
        var env = await _context.Environments
            .Include(e => e.Application)
            .FirstOrDefaultAsync(e => e.Name == envName && e.Application.Name == appName);
            
        if (env == null) return NotFound("Environment not found");

        var existing = await _context.ConfigItems
            .FirstOrDefaultAsync(c => c.EnvironmentId == env.Id && c.Key == item.Key);

        if (existing != null)
        {
            existing.Value = item.Value;
        }
        else
        {
            item.EnvironmentId = env.Id;
            _context.ConfigItems.Add(item);
        }

        await _context.SaveChangesAsync();
        return Ok(item);
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteConfig(int id)
    {
        var existing = await _context.ConfigItems.FindAsync(id);

        if (existing == null) return NotFound("Config item not found");

        _context.ConfigItems.Remove(existing);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}
