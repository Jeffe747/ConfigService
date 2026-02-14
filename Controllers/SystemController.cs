using Microsoft.AspNetCore.Mvc;
using ConfigService.Data;
using Microsoft.EntityFrameworkCore;

namespace ConfigService.Controllers;

[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    private readonly ConfigContext _context;

    public SystemController(ConfigContext context)
    {
        _context = context;
    }

    [HttpGet("key")]
    public async Task<IActionResult> GetSystemKey()
    {
        var key = await _context.SystemOptions
            .Where(o => o.Key == "GlobalAiApiKey")
            .Select(o => o.Value)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(key))
        {
            return NotFound("Global System Key not found");
        }

        return Ok(new { Key = key });
    }
}
