using System.Text.Json.Serialization;

namespace backend.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [JsonIgnore]
    public List<User>? Users { get; set; }
}