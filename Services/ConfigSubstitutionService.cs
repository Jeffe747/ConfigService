using System.Text.Json;
using System.Text.Json.Nodes;
using ConfigService.Data;
using Microsoft.EntityFrameworkCore;

namespace ConfigService.Services;

public interface IConfigSubstitutionService
{
    Task<string> SubstituteAsync(string appName, string envName, string jsonTemplate);
}

public class ConfigSubstitutionService : IConfigSubstitutionService
{
    private readonly ConfigContext _context;

    public ConfigSubstitutionService(ConfigContext context)
    {
        _context = context;
    }

    public async Task<string> SubstituteAsync(string appName, string envName, string jsonTemplate)
    {
        // 1. Fetch Config for App/Env
        var configItems = await _context.ConfigItems
            .Where(c => c.Environment.Name == envName && c.Environment.Application.Name == appName)
            .ToDictionaryAsync(c => c.Key, c => c.Value);

        if (!configItems.Any())
        {
            // No config found, return original or throwing? 
            // Return original for now, maybe log warning.
            return jsonTemplate;
        }

        // 2. Parse JSON
        var root = JsonNode.Parse(jsonTemplate);
        if (root == null) return jsonTemplate;

        // 3. Traverse and Replace
        ReplaceValues(root, "", configItems);

        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private void ReplaceValues(JsonNode node, string currentPath, Dictionary<string, string> configItems)
    {
        if (node is JsonObject obj)
        {
            foreach (var property in obj.ToList()) // ToList to allow modification
            {
                var key = property.Key;
                var path = string.IsNullOrEmpty(currentPath) ? key : $"{currentPath}:{key}";
                
                if (property.Value is JsonValue val)
                {
                    if (val.ToString().StartsWith("$"))
                    {
                        var valueKey = val.ToString().Substring(1);
                        var configPath = string.IsNullOrEmpty(currentPath) ? valueKey : $"{currentPath}:{valueKey}";

                        // Check if exact match
                        if (configItems.TryGetValue(valueKey, out var newValue))
                        {
                            obj[key] = JsonValue.Create(newValue);
                        }
                    }
                }
                else if (property.Value != null)
                {
                    ReplaceValues(property.Value, path, configItems);
                }
            }
        }
        else if (node is JsonArray arr)
        {
            // Indices in arrays are not typically used for config substitution like "Array:0:Item" in standard appsettings
            // But if needed, we could implement index tracking.
            // For now, let's skip array index substitution unless requested.
            for (int i = 0; i < arr.Count; i++)
            {
                if (arr[i] != null)
                {
                    ReplaceValues(arr[i]!, currentPath, configItems); // Path doesn't change for array items usually in this simple logic? 
                    // Actually, typically appsettings is "Array:0", "Array:1"
                    // Let's implement index path
                     // var path = $"{currentPath}:{i}";
                     // ReplaceValues(arr[i]!, path, configItems);
                }
            }
        }
    }
}
