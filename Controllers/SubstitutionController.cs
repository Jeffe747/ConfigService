using Microsoft.AspNetCore.Mvc;
using ConfigService.Services;
using ConfigService.Data;

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

    [HttpPost("{appName}/{envName}")]
    public async Task<IActionResult> Substitute(string appName, string envName, [FromBody] object jsonTemplate)
    {
        // 1. Auth Check (Global System Key)
        if (!IsAuthorized())
        {
            return Unauthorized("Invalid X-System-Key");
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
