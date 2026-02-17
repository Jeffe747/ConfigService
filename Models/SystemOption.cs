namespace ConfigService.Models;

public class SystemOption
{
    public int Id { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(450)]
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
