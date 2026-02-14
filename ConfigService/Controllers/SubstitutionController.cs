using Microsoft.AspNetCore.Mvc;
using ConfigService.Services;
using ConfigService.Data;
using Microsoft.EntityFrameworkCore;

namespace ConfigService.Controllers;

[ApiController]
[Route("config")]
public class SubstitutionController : ControllerBase
{
    private readonly IConfigSubstitutionService _substitutionService;
    private readonly ConfigContext _context;

    public SubstitutionController(IConfigSubstitutionService substitutionService, ConfigContext context)
    {
        _substitutionService = substitutionService;
        _context = context;
    }

    [HttpPost("{appName}/{envName}")]
    public async Task<IActionResult> Substitute(string appName, string envName, [FromBody] object jsonTemplate)
    {
        // 1. Auth Check (API Key)
        if (!Request.Headers.TryGetValue("X-App-Key", out var apiKey))
        {
            return Unauthorized("Missing X-App-Key header");
        }

        var app = await _context.Applications.FirstOrDefaultAsync(a => a.Name == appName);
        if (app == null || app.ApiKey != apiKey.ToString())
        {
            return Unauthorized("Invalid API Key");
        }

        // 2. Perform Substitution
        var jsonString = jsonTemplate.ToString(); 
        // Note: [FromBody] object parses it. We want raw string or use the object. 
        // If we use 'object', System.Text.Json deserializes it to JsonElement.
        
        // Better way: Read body as string to preserve formatting/comments if possible? 
        // Or re-serialize. Re-serializing usage System.Text.Json is safer.
        string inputJson = jsonTemplate.ToString() ?? "";
        
        var result = await _substitutionService.SubstituteAsync(appName, envName, inputJson);
        
        // 3. Return
        return Content(result, "application/json");
    }
}
