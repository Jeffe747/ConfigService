namespace ConfigService.Models;

public class Application
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Environment> Environments { get; set; } = new();
}
