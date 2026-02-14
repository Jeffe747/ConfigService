namespace ConfigService.Models;

public class Environment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public int ApplicationId { get; set; }
    public Application? Application { get; set; }
    
    public List<ConfigItem> ConfigItems { get; set; } = new();
}
