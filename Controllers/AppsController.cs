using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConfigService.Data;
using ConfigService.Models;

namespace ConfigService.Controllers;

[ApiController]
[Route("api/apps")]
public class AppsController : ControllerBase
{
    private readonly ConfigContext _context;

    public AppsController(ConfigContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Application>>> GetApps()
    {
        return await _context.Applications.Include(a => a.Environments).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Application>> GetApp(int id)
    {
        var app = await _context.Applications.Include(a => a.Environments).FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return NotFound();
        return app;
    }

    [HttpPost]
    public async Task<ActionResult<Application>> CreateApp(Application app)
    {
        if (string.IsNullOrWhiteSpace(app.Name)) return BadRequest("Name required");

        _context.Applications.Add(app);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetApp), new { id = app.Id }, app);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApp(int id)
    {
        var app = await _context.Applications.FindAsync(id);
        if (app == null) return NotFound();

        _context.Applications.Remove(app);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{appName}/envs")]
    public async Task<ActionResult<IEnumerable<Models.Environment>>> GetEnvs(string appName)
    {
        var app = await _context.Applications.Include(a => a.Environments).FirstOrDefaultAsync(a => a.Name == appName);
        if (app == null) return NotFound("App not found");
        return app.Environments;
    }

    [HttpPost("{appName}/envs")]
    public async Task<ActionResult<Models.Environment>> CreateEnv(string appName, Models.Environment env)
    {
        try
        {
            var app = await _context.Applications.FirstOrDefaultAsync(a => a.Name == appName);
            if (app == null) return NotFound("App not found");

            env.ApplicationId = app.Id;
            _context.Environments.Add(env);
            await _context.SaveChangesAsync();

            return Ok(env);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating env: {ex.Message} \n {ex.InnerException?.Message}");
        }
    }



}
